using RvB.Linq.Utils;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<SkipLast<TIterator, TSource>, TSource> SkipLast<TIterator, TSource>(this Iterable<TIterator, TSource> source, int skip)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, skip));
    }
}

public ref struct SkipLast<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
    private enum State
    {
        Uninitialzed = 0,
        Read = 1,
        Finished = 2
    }
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly int _skipCount;
    private ValueQueue<TSource> _queue;
    private State _state;
    private int _readCount;

    public SkipLast(TIterator source, int skipCount)
    {
        _source = source;
        _skipCount = int.Max(0, skipCount);
    }

    public bool TryGetCount(out int count)
    {
        if (_source.TryGetCount(out count)) {
            count = int.Max(0, count - _skipCount);
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_source.TryGetCount(out var count)) {
            var offset = index.GetOffset(count);
            if (offset >= 0 && offset < count - _skipCount) {
                return _source.TryGet(offset, out item);
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_source.TryGetCount(out var count)) {
            if (_state == State.Uninitialzed) {
                _readCount = int.Max(0, count - _skipCount);
                _state = State.Read;
            }
            if (_state == State.Read) {
                if (_readCount > 0 && _source.TryGetNext(out current)) {
                    _readCount -= 1;
                    return true;
                }
                _state = State.Finished;
            }
        } else {
            if (_state == State.Uninitialzed) {
                var size = _skipCount;
                _queue = new(size);
                while (size > 0 && _source.TryGetNext(out var item)) {
                    _queue.Enqueue(item);
                    size -= 1;
                }
                if (size > 0) {
                    _state = State.Finished;
                } else {
                    _state = State.Read;
                }
            }
            if (_state == State.Read) {
                if (_source.TryGetNext(out var item)) {
                    if (_queue.Count > 0) {
                        current = _queue.DequeueEnqueue(item);
                    } else {
                        current = item;
                    }
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
