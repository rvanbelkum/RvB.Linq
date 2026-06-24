using RvB.Linq.Utils;
using System.Runtime.InteropServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static List<TSource> ToList<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        => ToList<TIterator, TSource>(source._iterator);

    public static List<TSource> ToList<TIterator, TSource>(this TIterator iterator)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        if (iterator.TryGetCount(out var count)) {
            var list = new List<TSource>(count); // list with capacity set internal buffer as source size
            CollectionsMarshal.SetCount(list, count);
            var span = CollectionsMarshal.AsSpan(list);
            var i = 0;
            while (iterator.TryGetNext(out var current)) {
                span[i] = current;
                i++;
            }
            return list;
        } else {
            List<TSource> list;
            using (var arrayBuilder = new ArrayBuilder<TSource>()) {
                while (iterator.TryGetNext(out var item)) {
                    arrayBuilder.Add(item);
                }
                count = arrayBuilder.Count;
                list = new(count);
                CollectionsMarshal.SetCount(list, count);

                var listSpan = CollectionsMarshal.AsSpan(list);
                arrayBuilder.CopyTo(listSpan);
            }
            return list;
        }
    }
}
