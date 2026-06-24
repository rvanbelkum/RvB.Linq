namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static int Count<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            if (iterator.TryGetCount(out var count)) {
                return count;
            }
            count = 0;
            while (iterator.TryGetNext(out _)) {
                checked { count += 1; }
            }
            return count;
        }
    }

    public static int Count<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Where(predicate).Count();
    }
}
