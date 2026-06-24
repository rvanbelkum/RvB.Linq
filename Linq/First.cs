using RvB.Linq.Utils;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource First<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return iterator.TryGetNext(out var value) ? value : Throws.NoElements<TSource>();
        }
    }

    public static TSource First<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            while (iterator.TryGetNext(out var value)) {
                if (predicate(value)) {
                    return value;
                }
            }
            return Throws.NoElements<TSource>();
        }
    }

    public static TSource? FirstOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return iterator.TryGetNext(out var value) ? value : default;
        }
    }

    public static TSource FirstOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, TSource defaultValue)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return iterator.TryGetNext(out var value) ? value : defaultValue;
        }
    }

    public static TSource? FirstOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            while (iterator.TryGetNext(out var value)) {
                if (predicate(value)) {
                    return value;
                }
            }
            return default;
        }
    }

    public static TSource FirstOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            while (iterator.TryGetNext(out var value)) {
                if (predicate(value)) {
                    return value;
                }
            }
            return defaultValue;
        }
    }
}
