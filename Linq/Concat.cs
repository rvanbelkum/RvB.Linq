using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Concat<TIterator, TIterator1, TSource>, TSource> Concat<TIterator, TIterator1, TSource>(this Iterable<TIterator, TSource> source, Iterable<TIterator1, TSource> source1)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TIterator1 : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, source1._iterator));
    }
}

public ref struct Concat<TIterator1, TIterator2, TSource> : IIterator<TSource>
    where TIterator1 : struct, IIterator<TSource>, allows ref struct
    where TIterator2 : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator1 _firstSource;
    private TIterator2 _secondSource;
#pragma warning restore IDE0044 // Add readonly modifier

    public Concat(TIterator1 firstSource, TIterator2 secondSource)
    {
        _firstSource = firstSource;
        _secondSource = secondSource;
    }

    public bool TryGetCount(out int count)
    {
        if (_firstSource.TryGetCount(out var count1) && _secondSource.TryGetCount(out var count2)) {
            checked { count = count1 + count2; }
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_firstSource.TryGetCount(out var count1) && _secondSource.TryGetCount(out var count2)) {
            if (index.IsFromEnd) {
                if (index.Value <= count2) {
                    return _secondSource.TryGet(index, out item);
                }
                return _firstSource.TryGet(index.Value - count2, out item);
            } else {
                if (index.Value < count1) {
                    return _firstSource.TryGet(index, out item);
                }
                return _secondSource.TryGet(index.Value - count1, out item);
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_firstSource.TryGetNext(out current)) {
            return true;
        }
        if (_secondSource.TryGetNext(out current)) {
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _firstSource.Dispose();
        _secondSource.Dispose();
    }
}
