namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static bool Contains<TIterator, TSource>(this Iterable<TIterator, TSource> source, TSource value, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        comparer ??= EqualityComparer<TSource>.Default;
        using (var iterator = source._iterator) {
            while (iterator.TryGetNext(out var item)) {
                if (comparer.Equals(item, value)) {
                    return true;
                }
            }
            return false;
        }
    }
}
