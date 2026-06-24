using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<DistinctBy<TIterator, TSource, TKey>, TSource> DistinctBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, keySelector, comparer));
    }
}

public ref struct DistinctBy<TIterator, TSource, TKey> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IEqualityComparer<TKey>? _comparer;
    private HashSet<TKey>? _visited;

    public DistinctBy(TIterator source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        _source = source;
        _keySelector = keySelector;
        _comparer = comparer;
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
        _visited ??= _source.TryGetCount(out var count) ? new(count, _comparer) : new(_comparer);
        while (_source.TryGetNext(out current)) {
            if (_visited.Add(_keySelector(current))) {
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
