namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static bool All<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            while (iterator.TryGetNext(out var item)) {
                if (!predicate(item)) {
                    return false;
                }
            }
            return true;
        }
    }
}
