using RvB.Linq.Utils;
using System.Buffers;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource[] ToArray<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        var array = IteratorExtensions.ToArray<TIterator, TSource>(ref source._iterator);
        source._iterator.Dispose();
        return array;
    }
}

internal static class IteratorExtensions
{
    internal static (int Count, TSource[] Array) ToArrayFromPool<TIterator, TSource>(ref TIterator iterator)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return ToArrayInternal<TIterator, TSource>(ref iterator, true);
    }

    internal static TSource[] ToArray<TIterator, TSource>(ref TIterator iterator)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return ToArrayInternal<TIterator, TSource>(ref iterator, false).Array;
    }

    internal static (int Count, TSource[] Array) ToArrayInternal<TIterator, TSource>(ref TIterator iterator, bool rented)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        TSource[] array;
        if (iterator.TryGetCount(out int count)) {
            if (count == 0) {
                return (0, []);
            }
            if (rented) {
                array = ArrayPool<TSource>.Shared.Rent(count);
            } else {
                array = GC.AllocateUninitializedArray<TSource>(count);
            }
            var index = 0;
            while (iterator.TryGetNext(out var item)) {
                array[index] = item;
                index += 1;
            }
        } else {
            if (!iterator.TryGetNext(out var item)) {
                return (0, []);
            }
            using (var arrayBuilder = new ArrayBuilder<TSource>()) {
                do {
                    arrayBuilder.Add(item);
                } while (iterator.TryGetNext(out item));
                count = arrayBuilder.Count;
                array = arrayBuilder.ToArray(rented);
            }
        }
        return (count, array);
    }
}
