namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Dictionary<TKey, TSource> ToDictionary<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TKey : notnull
    {
        using (var iterator = source._iterator) {
            comparer ??= EqualityComparer<TKey>.Default;
            var dictionary = iterator.TryGetCount(out var count) ? new Dictionary<TKey, TSource>(count, comparer) : new(comparer);
            while (iterator.TryGetNext(out var next)) {
                dictionary[keySelector(next)] = next;
            }
            return dictionary;
        }
    }

    public static Dictionary<TKey, TElement> ToDictionary<TIterator, TSource, TKey, TElement>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            comparer ??= EqualityComparer<TKey>.Default;
            var dictionary = iterator.TryGetCount(out var count) ? new Dictionary<TKey, TElement>(count, comparer) : new(comparer);
            while (iterator.TryGetNext(out var next)) {
                dictionary[keySelector(next)] = elementSelector(next);
            }
            return dictionary;
        }
    }
}
