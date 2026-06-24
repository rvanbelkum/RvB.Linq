using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Join<TIterator, TIterator2, TOuter, TInner, TKey, TResult>, TResult> Join<TIterator, TIterator2, TOuter, TInner, TKey, TResult>(this Iterable<TIterator, TOuter> source, Iterable<TIterator2, TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector)
        where TIterator : struct, IIterator<TOuter>, allows ref struct
        where TIterator2 : struct, IIterator<TInner>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, inner._iterator, outerKeySelector, innerKeySelector, resultSelector, null));
    }

    public static Iterable<Join<TIterator, TIterator2, TOuter, TInner, TKey, TResult>, TResult> Join<TIterator, TIterator2, TOuter, TInner, TKey, TResult>(this Iterable<TIterator, TOuter> source, Iterable<TIterator2, TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TOuter>, allows ref struct
        where TIterator2 : struct, IIterator<TInner>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, inner._iterator, outerKeySelector, innerKeySelector, resultSelector, comparer));
    }

    public static Iterable<Join<TIterator, EnumerableIterator<TInner>, TOuter, TInner, TKey, TResult>, TResult> Join<TIterator, TOuter, TInner, TKey, TResult>(this Iterable<TIterator, TOuter> source, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TOuter>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, inner.AsIterable()._iterator, outerKeySelector, innerKeySelector, resultSelector, comparer));
    }
}

public ref struct Join<TIterator, TIterator2, TOuter, TInner, TKey, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TOuter>, allows ref struct
    where TIterator2 : struct, IIterator<TInner>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
    private TIterator2 _inner;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TOuter, TKey> _outerKeySelector;
    private readonly Func<TInner, TKey> _innerKeySelector;
    private readonly Func<TOuter, TInner, TResult> _resultSelector;
    private readonly IEqualityComparer<TKey>? _comparer;

    private Lookup<TKey, TInner>? _innerLookup;
    private Grouping<TKey, TInner>? _currentGroup;
    private int _currentGroupIndex;
    private TOuter _currentOuter = default!;

    public Join(TIterator source, TIterator2 inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
    {
        _source = source;
        _inner = inner;
        _outerKeySelector = outerKeySelector;
        _innerKeySelector = innerKeySelector;
        _resultSelector = resultSelector;
        _comparer = comparer;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TResult item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        if (_innerLookup == null) {
            _innerLookup = Lookup<TKey, TInner>.CreateForJoin(_inner, _innerKeySelector, _comparer);
        }
        if (_innerLookup.Count == 0) {
            goto END;
        }
        // iterating group
    ITERATE:
        if (_currentGroup != null) {
            if (_currentGroupIndex < _currentGroup.Count) {
                current = _resultSelector(_currentOuter, _currentGroup[_currentGroupIndex]);
                _currentGroupIndex++;
                return true;
            } else {
                _currentGroup = null;
            }
        }

        while (_source.TryGetNext(out var value)) {
            var key = _outerKeySelector(value);
            if (key is not null) // Enumerable.Join ignores null keys
            {
                if (_innerLookup.TryGetGrouping(key, out var group)) {
                    _currentOuter = value;
                    _currentGroup = group;
                    _currentGroupIndex = 0;
                    goto ITERATE;
                }
            }
        }
    END:
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _inner.Dispose();
        _source.Dispose();
    }
}
