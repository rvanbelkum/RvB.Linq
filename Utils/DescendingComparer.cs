namespace RvB.Linq.Utils;

internal class DescendingComparer<TSource> : IComparer<TSource>
{
    public static DescendingComparer<TSource> Default { get; } = new();

    private DescendingComparer() { }

    public int Compare(TSource? x, TSource? y)
        => Comparer<TSource>.Default.Compare(y, x);
}
