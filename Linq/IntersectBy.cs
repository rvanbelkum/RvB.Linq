using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<IntersectBy<TIterator, TIterator1, TSource, TKey>, TSource> IntersectBy<TIterator, TIterator1, TSource, TKey>(this Iterable<TIterator, TSource> source, Iterable<TIterator1, TKey> source1, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TIterator1 : struct, IIterator<TKey>, allows ref struct
    {
        return new(new(source._iterator, source1, keySelector, comparer));
    }
}

public ref struct IntersectBy<TIterator1, TIterator2, TSource, TKey> : IIterator<TSource>
    where TIterator1 : struct, IIterator<TSource>, allows ref struct
    where TIterator2 : struct, IIterator<TKey>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator1 _firstSource;
    private Iterable<TIterator2, TKey> _secondSource;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IEqualityComparer<TKey>? _comparer;
    private HashSet<TKey>? _set2;

    public IntersectBy(TIterator1 firstSource, Iterable<TIterator2, TKey> secondSource, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
    {
        _firstSource = firstSource;
        _secondSource = secondSource;
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
        _set2 ??= _secondSource.ToHashSet(_comparer);
        while (_firstSource.TryGetNext(out var item)) {
            if (_set2.Remove(_keySelector(item))) {
                current = item;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _firstSource.Dispose();
    }
}
