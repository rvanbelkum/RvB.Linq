namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static TSource ElementAt<TIterator, TSource>(this Iterable<TIterator, TSource> source, int index)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        if (index >= 0 && source.TryGet(index, out var element)) {
            return element;
        }
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public static TSource ElementAt<TIterator, TSource>(this Iterable<TIterator, TSource> source, Index index)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        if ((!index.IsFromEnd || index.Value != 0) && source.TryGet(index, out var element)) {
            return element;
        }
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public static TSource ElementAtOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, int index)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        if (index >= 0 && source.TryGet(index, out var element)) {
            return element;
        }
        return default!;
    }

    public static TSource ElementAtOrDefault<TIterator, TSource>(this Iterable<TIterator, TSource> source, Index index)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        if ((!index.IsFromEnd || index.Value != 0) && source.TryGet(index, out var element)) {
            return element;
        }
        return default!;
    }
}
