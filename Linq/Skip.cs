using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Skip<TIterator, TSource>, TSource> Skip<TIterator, TSource>(this Iterable<TIterator, TSource> source, int skip)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, skip));
    }
}

public ref struct Skip<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly int _skipCount;
    private int _skipped;

    public Skip(TIterator source, int skipCount)
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
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_source.TryGetCount(out var count)) {
            var offset = index.GetOffset(count) + _skipCount;
            if ((uint)offset < (uint)count) {
                return _source.TryGet(offset, out item);
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        while (_skipped < _skipCount) {
            if (!_source.TryGetNext(out _)) {
                break;
            }
            _skipped += 1;
        }
        return _source.TryGetNext(out current);
    }

    public void Dispose() => _source.Dispose();
}
