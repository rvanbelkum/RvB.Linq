using RvB.Linq.Utils;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource Last<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return TryGetLast(iterator, out TSource value) ? value : Throws.NoElements<TSource>();
        }
    }

    public static TSource Last<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return TryGetLast(iterator, predicate, out TSource value) ? value : Throws.NoElements<TSource>();
        }
    }

    public static TSource? LastOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return TryGetLast(iterator, out TSource value) ? value : default;
        }
    }

    public static TSource LastOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, TSource defaultValue)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return TryGetLast(iterator, out TSource value) ? value : defaultValue;
        }
    }

    public static TSource? LastOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return TryGetLast(iterator, predicate, out TSource value) ? value : default!;
        }
    }

    public static TSource LastOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        using (var iterator = source._iterator) {
            return TryGetLast(iterator, predicate, out TSource value) ? value : defaultValue;
        }
    }

    private static bool TryGetLast<TIterator, TSource>(TIterator iterator, out TSource last)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        if (iterator.TryGet(^1, out last)) {
            return true;
        }
        if (iterator.TryGetNext(out last)) {
            while (iterator.TryGetNext(out var item)) {
                last = item;
            }
            return true;
        }
        Unsafe.SkipInit(out last);
        return false;
    }


    private static bool TryGetLast<TIterator, TSource>(TIterator iterator, Func<TSource, bool> predicate, out TSource last)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        while (iterator.TryGetNext(out last)) {
            if (predicate(last)) {
                while (iterator.TryGetNext(out var item)) {
                    if (predicate(item)) {
                        last = item;
                    }
                }
                return true;
            }
        }
        Unsafe.SkipInit(out last);
        return false;
    }
}
