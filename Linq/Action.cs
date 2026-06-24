namespace RvB.Linq.Linq;

public static partial class IterableExtensions
{
    public static void Action<TIterator, TSource>(this Iterable<TIterator, TSource> source, Action<TSource> action)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            while (iterator.TryGetNext(out var item)) {
                action(item);
            }
        }
    }
}
