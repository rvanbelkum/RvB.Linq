using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<GroupJoin<TOuterIterator, TInnerIterator, TOuter, TInner, TKey, TResult>, TResult> GroupJoin<TOuterIterator, TInnerIterator, TOuter, TInner, TKey, TResult>(
        this Iterable<TOuterIterator, TOuter> source,
        Iterable<TInnerIterator, TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TInnerIterator : struct, IIterator<TInner>, allows ref struct
        where TOuterIterator : struct, IIterator<TOuter>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, inner._iterator, outerKeySelector, innerKeySelector, resultSelector, comparer));
    }

    public static Iterable<GroupJoin<TOuterIterator, TOuter, TInner, TKey, TResult>, TResult> GroupJoin<TOuterIterator, TOuter, TInner, TKey, TResult>(
        this Iterable<TOuterIterator, TOuter> source,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TOuterIterator : struct, IIterator<TOuter>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, inner, outerKeySelector, innerKeySelector, resultSelector, comparer));
    }
}

public ref struct GroupJoin<TOuterIterator, TInnerIterator, TOuter, TInner, TKey, TResult>
    : IIterator<TResult>
    where TInnerIterator : struct, IIterator<TInner>, allows ref struct
    where TOuterIterator : struct, IIterator<TOuter>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TOuterIterator _outer;
    private TInnerIterator _inner;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TOuter, TKey> _outerKeySelector;
    private readonly Func<TInner, TKey> _innerKeySelector;
    private readonly Func<TOuter, IEnumerable<TInner>, TResult> _resultSelector;
    private readonly IEqualityComparer<TKey> _comparer;

    private Lookup<TKey, TInner>? _innerLookup;

    public GroupJoin(
        TOuterIterator outer,
        TInnerIterator inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer)
    {
        _outer = outer;
        _inner = inner;
        _outerKeySelector = outerKeySelector;
        _innerKeySelector = innerKeySelector;
        _resultSelector = resultSelector;
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
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
        while (_outer.TryGetNext(out var value)) {
            var key = _outerKeySelector(value);
            // Enumerable.GroupJoin allows null unlike Join
            if (_innerLookup.TryGetGrouping(key, out var grouping)) {
                current = _resultSelector(value, grouping);
            } else {
                current = _resultSelector(value, []);
            }
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _inner.Dispose();
        _outer.Dispose();
    }
}

public ref struct GroupJoin<TOuterIterator, TOuter, TInner, TKey, TResult>
    : IIterator<TResult>
    where TOuterIterator : struct, IIterator<TOuter>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TOuterIterator _outer;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly IEnumerable<TInner> _inner;
    private readonly Func<TOuter, TKey> _outerKeySelector;
    private readonly Func<TInner, TKey> _innerKeySelector;
    private readonly Func<TOuter, IEnumerable<TInner>, TResult> _resultSelector;
    private readonly IEqualityComparer<TKey> _comparer;

    private Lookup<TKey, TInner>? _innerLookup;

    public GroupJoin(
        TOuterIterator outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer)
    {
        _outer = outer;
        _inner = inner;
        _outerKeySelector = outerKeySelector;
        _innerKeySelector = innerKeySelector;
        _resultSelector = resultSelector;
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
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
        if (_outer.TryGetNext(out var value)) {
            var key = _outerKeySelector(value);
            // Enumerable.GroupJoin allows null unlike Join
            if (_innerLookup.TryGetGrouping(key, out var grouping)) {
                current = _resultSelector(value, grouping);
            } else {
                current = _resultSelector(value, []);
            }
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _outer.Dispose();
    }
}
