using RvB.Linq.Utils;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<TakeLast<TIterator, TSource>, TSource> TakeLast<TIterator, TSource>(this Iterable<TIterator, TSource> source, int count)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, count));
    }
}

public ref struct TakeLast<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
    private enum State
    {
        Uninitialized,
        Buffered,
        Finished
    }
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private int? _count;
    private int? _skipCount;
    private int _takeCount;
    private State _state = State.Uninitialized;
    private ValueQueue<TSource> _queue;

    public TakeLast(TIterator source, int takeCount)
    {
        _source = source;
        _takeCount = int.Max(0, takeCount);
        if (_source.TryGetCount(out var count)) {
            _skipCount = int.Max(0, count - _takeCount);
            _count = int.Min(count, _takeCount);
        }
    }

    public readonly bool TryGetCount(out int count)
    {
        if (_count.HasValue) {
            count = _count.Value;
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_source.TryGetCount(out var count)) {
            var takeCount = Math.Min(count, _takeCount);
            if (takeCount > 0) {
                var takeLastStartIndex = count - takeCount;
                var offsetInTakeLast = index.GetOffset(takeCount);
                if ((uint)offsetInTakeLast < (uint)takeCount) {
                    var sourceOffset = takeLastStartIndex + offsetInTakeLast;
                    return _source.TryGet(sourceOffset, out item);
                }
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_skipCount.HasValue) {
            while (_skipCount > 0 && _source.TryGetNext(out _)) {
                _skipCount -= 1;
            }
            if (_source.TryGetNext(out current)) {
                return true;
            }
        } else {
            if (_state == State.Uninitialized) {
                if (_takeCount > 0) {
                    _queue = new(16, _takeCount);
                    while (_source.TryGetNext(out var item)) {
                        _queue.Enqueue(item);
                    }
                }
                _state = State.Buffered;
            }
            if (_state == State.Buffered) {
                if (_queue.TryDequeue(out current)) {
                    return true;
                }
                _state = State.Finished;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}
