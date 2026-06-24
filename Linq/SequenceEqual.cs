namespace RvB.Linq;

public partial class IterableExtensions
{
    public static bool SequenceEqual<TIterator, TSource>(this Iterable<TIterator, TSource> source, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(second);
        return SequenceEqual(source, second.AsIterable(), comparer);
    }

    public static bool SequenceEqual<TIterator, TIterator2, TSource>(this Iterable<TIterator, TSource> source, Iterable<TIterator2, TSource> second, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TIterator2 : struct, IIterator<TSource>, allows ref struct
    {
        using var e1 = source._iterator;
        using var e2 = second._iterator;

        if (e1.TryGetCount(out var count1) && e2.TryGetCount(out var count2) && count1 != count2) {
            return false;
        }

        //if (e1.TryGetSpan(out var sourceSpan) && e2.TryGetSpan(out var secondSpan)) {
        //    return sourceSpan.SequenceEqual(secondSpan, comparer);
        //}

        comparer ??= EqualityComparer<TSource>.Default;
        while (e1.TryGetNext(out var value1)) {
            if (!e2.TryGetNext(out var value2) || !comparer.Equals(value1, value2)) {
                return false;
            }
        }
        return !e2.TryGetNext(out _);
    }
}
