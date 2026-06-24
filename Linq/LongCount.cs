namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static long LongCount<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            if (iterator.TryGetCount(out var count)) {
                return count;
            }
            var longCount = 0L;
            while (iterator.TryGetNext(out _)) {
                checked { longCount++; }
            }
            return longCount;
        }
    }

    public static long LongCount<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            var longCount = 0L;
            while (iterator.TryGetNext(out var item)) {
                if (predicate(item)) {
                    checked { longCount++; }
                }
            }
            return longCount;
        }
    }
}
