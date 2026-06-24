namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static bool Any<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            if (iterator.TryGetCount(out var count)) {
                return count > 0;
            }
            if (iterator.TryGetNext(out _)) {
                return true;
            }
            return false;
        }
    }

    public static bool Any<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            while (iterator.TryGetNext(out var item)) {
                if (predicate(item)) {
                    return true;
                }
            }
            return false;
        }
    }
}
