namespace RvB.Linq;

public static partial class IterableExtensions
{
    private static TResult? Average<TIterator, T, TResult>(Iterable<TIterator, T?> source)
        where TIterator : struct, IIterator<T?>, allows ref struct
        where T : struct, INumber<T>
        where TResult : struct, INumber<TResult>
    {
        using (var iterator = source._iterator) {
            var sum = TResult.Zero;
            var count = TResult.Zero;
            while (iterator.TryGetNext(out var element)) {
                if (element is not null) {
                    sum += TResult.CreateChecked(element.Value);
                    count++;
                }
            }
            if (count == TResult.Zero) {
                return null;
            }
            return sum / count;
        }
    }

    private static TResult Average<TIterator, T, TResult>(Iterable<TIterator, T> source)
        where TIterator : struct, IIterator<T>, allows ref struct
        where T : INumber<T>
        where TResult : INumber<TResult>
    {
        using (var iterator = source._iterator) {
            var sum = TResult.Zero;
            var count = TResult.Zero;
            while (iterator.TryGetNext(out var element)) {
                sum += TResult.CreateChecked(element);
                count++;
            }
            if (count == TResult.Zero) {
                throw new InvalidOperationException("Sequence does not contain any alements");
            }
            return sum / count;
        }
    }

    public static double Average<TIterator>(this Iterable<TIterator, int> source)
        where TIterator : struct, IIterator<int>, allows ref struct
    {
        return Average<TIterator, int, double>(source);
    }

    public static double? Average<TIterator>(this Iterable<TIterator, int?> source)
        where TIterator : struct, IIterator<int?>, allows ref struct
    {
        return Average<TIterator, int, double>(source);
    }

    public static double Average<TIterator>(this Iterable<TIterator, long> source)
        where TIterator : struct, IIterator<long>, allows ref struct
    {
        return Average<TIterator, long, double>(source);
    }

    public static double? Average<TIterator>(this Iterable<TIterator, long?> source)
        where TIterator : struct, IIterator<long?>, allows ref struct
    {
        return Average<TIterator, long, double>(source);
    }

    public static float Average<TIterator>(this Iterable<TIterator, float> source)
        where TIterator : struct, IIterator<float>, allows ref struct
    {
        return Average<TIterator, float, float>(source);
    }

    public static float? Average<TIterator>(this Iterable<TIterator, float?> source)
        where TIterator : struct, IIterator<float?>, allows ref struct
    {
        return Average<TIterator, float, float>(source);
    }

    public static double Average<TIterator>(this Iterable<TIterator, double> source)
        where TIterator : struct, IIterator<double>, allows ref struct
    {
        return Average<TIterator, double, double>(source);
    }

    public static double? Average<TIterator>(this Iterable<TIterator, double?> source)
        where TIterator : struct, IIterator<double?>, allows ref struct
    {
        return Average<TIterator, double, double>(source);
    }

    public static decimal Average<TIterator>(this Iterable<TIterator, decimal> source)
        where TIterator : struct, IIterator<decimal>, allows ref struct
    {
        return Average<TIterator, decimal, decimal>(source);
    }

    public static decimal? Average<TIterator>(this Iterable<TIterator, decimal?> source)
        where TIterator : struct, IIterator<decimal?>, allows ref struct
    {
        return Average<TIterator, decimal, decimal>(source);
    }

    public static double Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, int> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static double? Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, int?> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static double Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, long> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static double? Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, long?> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static float Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, float> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static float? Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, float?> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static double Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, double> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static double? Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, double?> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static decimal Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, decimal> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }

    public static decimal? Average<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, decimal?> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return source.Select(selector).Average();
    }
}
