using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Intersect<TIterator, TIterator1, TSource>, TSource> Intersect<TIterator, TIterator1, TSource>(this Iterable<TIterator, TSource> source, Iterable<TIterator1, TSource> source1, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TIterator1 : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, source1, comparer));
    }
}

public ref struct Intersect<TIterator1, TIterator2, TSource> : IIterator<TSource>
    where TIterator1 : struct, IIterator<TSource>, allows ref struct
    where TIterator2 : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator1 _firstSource;
    private Iterable<TIterator2, TSource> _secondSource;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly IEqualityComparer<TSource>? _comparer;
    private HashSet<TSource>? _set2;

    public Intersect(TIterator1 firstSource, Iterable<TIterator2, TSource> secondSource, IEqualityComparer<TSource>? comparer = null)
    {
        _firstSource = firstSource;
        _secondSource = secondSource;
        _comparer = comparer;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        _set2 ??= _secondSource.ToHashSet(_comparer);
        while (_firstSource.TryGetNext(out var item)) {
            if (_set2.Remove(item)) {
                current = item;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _firstSource.Dispose();
    }
}
