using RvB.Linq.Utils;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource Aggregate<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, TSource, TSource> func)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(func);
        using (var iterator = source._iterator) {
            if (!iterator.TryGetNext(out var result)) {
                Throws.NoElements();
            }
            while (iterator.TryGetNext(out var next)) {
                result = func(result, next);
            }
            return result;
        }
    }

    public static TAccumulate Aggregate<TIterator, TSource, TAccumulate>(this Iterable<TIterator, TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(func);
        using (var iterator = source._iterator) {
            var result = seed;
            while (iterator.TryGetNext(out var next)) {
                result = func(result, next);
            }
            return result;
        }
    }

    public static TResult Aggregate<TIterator, TSource, TAccumulate, TResult>(this Iterable<TIterator, TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(resultSelector);
        using (var iterator = source._iterator) {
            var result = seed;
            while (iterator.TryGetNext(out var next)) {
                result = func(result, next);
            }
            return resultSelector(result);
        }
    }
}
