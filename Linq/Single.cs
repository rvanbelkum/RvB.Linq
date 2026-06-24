using RvB.Linq.Utils;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource Single<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            if (iterator.TryGetNext(out var value)) {
                if (iterator.TryGetNext(out _)) {
                    Throws.MoreThanOneElement();
                }
                return value;
            }
            return Throws.NoElements<TSource>();
        }
    }

    public static TSource Single<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        using (var iterator = source._iterator) {
            TSource value = default!;
            bool foundFirst = false;
            while (iterator.TryGetNext(out var item)) {
                if (predicate(item)) {
                    if (foundFirst) {
                        Throws.MoreThanOneMatch();
                    }
                    foundFirst = true;
                    value = item;
                }
            }
            if (foundFirst) {
                return value;
            }
            return Throws.NoMatch<TSource>();
        }
    }

    public static TSource? SingleOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            if (iterator.TryGetNext(out var value)) {
                if (iterator.TryGetNext(out _)) {
                    Throws.MoreThanOneElement();
                }
                return value;
            }
            return default!;
        }
    }

    public static TSource SingleOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, TSource defaultValue)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            if (iterator.TryGetNext(out var value)) {
                if (iterator.TryGetNext(out _)) {
                    Throws.MoreThanOneElement();
                }
                return value;
            }
            return defaultValue;
        }
    }

    public static TSource? SingleOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        using (var iterator = source._iterator) {
            TSource value = default!;
            bool foundFirst = false;
            while (iterator.TryGetNext(out var item)) {
                if (predicate(item)) {
                    if (foundFirst) {
                        Throws.MoreThanOneMatch();
                    }
                    foundFirst = true;
                    value = item;
                }
            }
            if (foundFirst) {
                return value;
            }
            return default;
        }
    }

    public static TSource SingleOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        using (var iterator = source._iterator) {
            TSource value = default!;
            bool foundFirst = false;
            while (iterator.TryGetNext(out var item)) {
                if (predicate(item)) {
                    if (foundFirst) {
                        Throws.MoreThanOneMatch();
                    }
                    foundFirst = true;
                    value = item;
                }
            }
            if (foundFirst) {
                return value;
            }
            return defaultValue;
        }
    }
}
