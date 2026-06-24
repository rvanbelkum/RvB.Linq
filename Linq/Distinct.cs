using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Distinct<TIterator, TSource>, TSource> Distinct<TIterator, TSource>(this Iterable<TIterator, TSource> source, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, comparer));
    }
}

public ref struct Distinct<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly HashSet<TSource> _visited;

    public Distinct(TIterator source, IEqualityComparer<TSource>? comparer)
    {
        _source = source;
        _visited = source.TryGetCount(out var count) ? new(count, comparer) : new(comparer);
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        while (_source.TryGetNext(out current)) {
            if (_visited.Add(current)) {
                return true;
            }
        }
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}
