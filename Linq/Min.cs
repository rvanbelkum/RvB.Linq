namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource? Min<TIterator, TSource>(this Iterable<TIterator, TSource> source, IComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            TSource? minElement = default;
            if (!iterator.TryGetNext(out var element)) {
                if (default(TSource) == null) {
                    return default;
                }
                throw new InvalidOperationException("Sequence does not contain any alements");
            }
            if (element is not null) {
                minElement = element;
            }
            comparer ??= Comparer<TSource>.Default;
            while (iterator.TryGetNext(out element)) {
                if (element is not null && comparer.Compare(element, minElement) < 0) {
                    minElement = element;
                }
            }
            return minElement;
        }
    }

    public static T? Min<TIterator, TSource, T>(this Iterable<TIterator, TSource> source, Func<TSource, T> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        return source.Select(selector).Min();
    }

    public static TSource? MinBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> selector, IComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));
        using (var iterator = source._iterator) {
            if (!iterator.TryGetNext(out var minElement)) {
                if (default(TSource) == null) {
                    return default;
                }
                throw new InvalidOperationException("Sequence does not contain any alements");
            }
            comparer ??= Comparer<TKey>.Default;
            var minKey = selector(minElement);

            if (default(TSource) is null) {
                if (minKey is null) {
                    var firstElement = minElement;
                    do {
                        if (!iterator.TryGetNext(out minElement)) {
                            return firstElement;
                        }
                        minKey = selector(minElement);
                    }
                    while (minKey is null);
                }
                while (iterator.TryGetNext(out var element)) {
                    var key = selector(element);
                    if (key is not null && comparer.Compare(key, minKey) < 0) {
                        minKey = key;
                        minElement = element;
                    }
                }
            } else {
                while (iterator.TryGetNext(out var element)) {
                    var key = selector(element);
                    if (comparer.Compare(key, minKey) < 0) {
                        minKey = key;
                        minElement = element;
                    }
                }
            }
            return minElement;
        }
    }
}
