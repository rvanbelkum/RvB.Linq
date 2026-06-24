namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource Sum<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TSource : struct, INumber<TSource>
    {
        using (var iterator = source._iterator) {
            TSource sum = TSource.Zero;
            while (iterator.TryGetNext(out var item)) {
                checked { sum += item; }
            }
            return sum;
        }
    }

    public static TSource? Sum<TIterator, TSource>(this Iterable<TIterator, TSource?> source)
        where TIterator : struct, IIterator<TSource?>, allows ref struct
        where TSource : struct, INumber<TSource>
    {
        using (var iterator = source._iterator) {
            TSource sum = TSource.Zero;
            while (iterator.TryGetNext(out var item)) {
                if (item != null) {
                    checked { sum += item.Value; }
                }
            }
            return sum;
        }
    }

    public static TResult Sum<TIterator, TSource, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TSource : struct, INumber<TSource>
        where TResult : struct, INumber<TResult>
    {
        using (var iterator = source._iterator) {
            TResult sum = TResult.Zero;
            while (iterator.TryGetNext(out var item)) {
                checked { sum += selector(item); }
            }
            return sum;
        }
    }

    public static TResult? Sum<TIterator, TSource, TResult>(this Iterable<TIterator, TSource?> source, Func<TSource, TResult> selector)
        where TIterator : struct, IIterator<TSource?>, allows ref struct
        where TSource : struct, INumber<TSource>
        where TResult : struct, INumber<TResult>
    {
        using (var iterator = source._iterator) {
            TResult sum = TResult.Zero;
            while (iterator.TryGetNext(out var item)) {
                if (item is not null) {
                    checked { sum += selector(item.Value); }
                }
            }
            return sum;
        }
    }

    public static TResult Sum<TIterator, TSource, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, int, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TSource : struct, INumber<TSource>
        where TResult : struct, INumber<TResult>
    {
        using (var iterator = source._iterator) {
            TResult sum = TResult.Zero;
            var index = 0;
            while (iterator.TryGetNext(out var item)) {
                checked { sum += selector(item, index++); }
            }
            return sum;
        }
    }

    public static TResult? Sum<TIterator, TSource, TResult>(this Iterable<TIterator, TSource?> source, Func<TSource, int, TResult> selector)
        where TIterator : struct, IIterator<TSource?>, allows ref struct
        where TSource : struct, INumber<TSource>
        where TResult : struct, INumber<TResult>
    {
        using (var iterator = source._iterator) {
            TResult sum = TResult.Zero;
            var index = 0;
            while (iterator.TryGetNext(out var item)) {
                if (item is not null) {
                    checked { sum += selector(item.Value, index); }
                }
                index++;
            }
            return sum;
        }
    }
}
