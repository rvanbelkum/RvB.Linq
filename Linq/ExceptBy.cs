using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<ExceptBy<TIterator, TIterator2, TSource, TKey>, TSource> ExceptBy<TIterator, TIterator2, TSource, TKey>(this Iterable<TIterator, TSource> source, Iterable<TIterator2, TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TIterator2 : struct, IIterator<TKey>, allows ref struct
    {
        return new(new(source._iterator, second, keySelector, comparer));
    }
}

public ref struct ExceptBy<TIterator, TIterator2, TSource, TKey> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TIterator2 : struct, IIterator<TKey>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
    private Iterable<TIterator2, TKey> _second;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IEqualityComparer<TKey>? _comparer;
    private HashSet<TKey>? _secondSet;

    public ExceptBy(TIterator source, Iterable<TIterator2, TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        _source = source;
        _second = second;
        _keySelector = keySelector;
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
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
        _secondSet ??= _second.ToHashSet(_comparer);
        while (_source.TryGetNext(out var value)) {
            if (_secondSet.Add(_keySelector(value))) {
                current = value;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}
