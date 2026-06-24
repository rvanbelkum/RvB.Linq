namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource? Max<TIterator, TSource>(this Iterable<TIterator, TSource> source, IComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            if (!iterator.TryGetNext(out var maxElement)) {
                if (default(TSource) == null) {
                    return default;
                }
                throw new InvalidOperationException("Sequence does not contain any alements");
            }
            comparer ??= Comparer<TSource>.Default;
            while (iterator.TryGetNext(out var element)) {
                if (comparer.Compare(element, maxElement) > 0) {
                    maxElement = element;
                }
            }
            return maxElement;
        }
    }

    public static T? Max<TIterator, TSource, T>(this Iterable<TIterator, TSource> source, Func<TSource, T> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return source.Select(selector).Max();
    }

    public static TSource? MaxBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> selector, IComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        using (var iterator = source._iterator) {
            if (!iterator.TryGetNext(out var maxElement)) {
                if (default(TSource) == null) {
                    return default;
                }
                throw new InvalidOperationException("Sequence does not contain any alements");
            }
            comparer ??= Comparer<TKey>.Default;
            var maxKey = selector(maxElement);

            if (default(TSource) is null) {
                if (maxKey is null) {
                    var firstElement = maxElement;
                    do {
                        if (!iterator.TryGetNext(out maxElement)) {
                            return firstElement;
                        }
                        maxKey = selector(maxElement);
                    }
                    while (maxKey is null);
                }
                while (iterator.TryGetNext(out var element)) {
                    var key = selector(element);
                    if (key is not null && comparer.Compare(key, maxKey) > 0) {
                        maxKey = key;
                        maxElement = element;
                    }
                }
            } else {
                while (iterator.TryGetNext(out var element)) {
                    var key = selector(element);
                    if (comparer.Compare(key, maxKey) > 0) {
                        maxKey = key;
                        maxElement = element;
                    }
                }
            }
            return maxElement;
        }
    }
}
