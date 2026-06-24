namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static HashSet<TSource> ToHashSet<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return ToHashSet<TIterator, TSource>(source._iterator);
    }

    public static HashSet<TSource> ToHashSet<TIterator, TSource>(this Iterable<TIterator, TSource> source, IEqualityComparer<TSource>? comparer)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return ToHashSet<TIterator, TSource>(source._iterator, comparer);
    }

    public static HashSet<TSource> ToHashSet<TIterator, TSource>(this TIterator iterator, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        var hashSet = iterator.TryGetCount(out var count) ? new HashSet<TSource>(count, comparer) : new(comparer);
        while (iterator.TryGetNext(out var item)) {
            hashSet.Add(item);
        }
        iterator.Dispose();
        return hashSet;
    }
}
