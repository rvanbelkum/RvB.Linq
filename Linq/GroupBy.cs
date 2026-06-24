using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<GroupBy<TIterator, TSource, TKey>, IGrouping<TKey, TSource>> GroupBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return new(new(source._iterator, keySelector, null));
    }

    public static Iterable<GroupBy<TIterator, TSource, TKey>, IGrouping<TKey, TSource>> GroupBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return new(new(source._iterator, keySelector, comparer));
    }

    public static Iterable<GroupBy2<TIterator, TSource, TKey, TElement>, IGrouping<TKey, TElement>> GroupBy<TIterator, TSource, TKey, TElement>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);
        return new(new(source._iterator, keySelector, elementSelector, null));
    }

    public static Iterable<GroupBy2<TIterator, TSource, TKey, TElement>, IGrouping<TKey, TElement>> GroupBy<TIterator, TSource, TKey, TElement>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);
        return new(new(source._iterator, keySelector, elementSelector, comparer));
    }

    public static Iterable<GroupBy3<TIterator, TSource, TKey, TResult>, TResult> GroupBy<TIterator, TSource, TKey, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, keySelector, resultSelector, null));
    }

    public static Iterable<GroupBy3<TIterator, TSource, TKey, TResult>, TResult> GroupBy<TIterator, TSource, TKey, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, keySelector, resultSelector, comparer));
    }

    public static Iterable<GroupBy4<TIterator, TSource, TKey, TElement, TResult>, TResult> GroupBy<TIterator, TSource, TKey, TElement, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, keySelector, elementSelector, resultSelector, null));
    }

    public static Iterable<GroupBy4<TIterator, TSource, TKey, TElement, TResult>, TResult> GroupBy<TIterator, TSource, TKey, TElement, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, keySelector, elementSelector, resultSelector, comparer));
    }
}

public ref struct GroupBy<TIterator, TSource, TKey> : IIterator<IGrouping<TKey, TSource>>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private bool _init;
    private IEnumerator<IGrouping<TKey, TSource>>? _lookupEnumerator;
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IEqualityComparer<TKey>? _comparer;

    public GroupBy(TIterator source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        _keySelector = keySelector;
        _comparer = comparer;
        _source = source;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out IGrouping<TKey, TSource> item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out IGrouping<TKey, TSource> current)
    {
        if (!_init) {
            _init = true;
            _lookupEnumerator = GetLookupEnumerator();
        }
        if (_lookupEnumerator!.MoveNext()) {
            current = _lookupEnumerator.Current;
            return true;
        }
        current = default!;
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }

    private readonly IEnumerator<IGrouping<TKey, TSource>> GetLookupEnumerator()
    {
        return Lookup<TKey, TSource>.Create(_source, _keySelector, _comparer).GetEnumerator();
    }
}

public ref struct GroupBy2<TIterator, TSource, TKey, TElement> : IIterator<IGrouping<TKey, TElement>>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private bool _init;
    private IEnumerator<IGrouping<TKey, TElement>>? _lookupEnumerator;
    private readonly Func<TSource, TKey> _keySelector;
    private readonly Func<TSource, TElement> _elementSelector;
    private readonly IEqualityComparer<TKey>? _comparer;

    public GroupBy2(TIterator source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
    {
        _keySelector = keySelector;
        _elementSelector = elementSelector;
        _comparer = comparer;
        _source = source;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out IGrouping<TKey, TElement> item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out IGrouping<TKey, TElement> current)
    {
        if (!_init) {
            _init = true;
            _lookupEnumerator = GetLookupEnumerator();
        }
        if (_lookupEnumerator!.MoveNext()) {
            current = _lookupEnumerator.Current;
            return true;
        }
        current = default!;
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }

    private readonly IEnumerator<IGrouping<TKey, TElement>> GetLookupEnumerator()
    {
        return Lookup<TKey, TElement>.Create(_source, _keySelector, _elementSelector, _comparer).GetEnumerator();
    }
}

public ref struct GroupBy3<TIterator, TSource, TKey, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private bool _init;
    private IEnumerator<IGrouping<TKey, TSource>>? _lookupEnumerator;
    private readonly Func<TSource, TKey> _keySelector;
    private readonly Func<TKey, IEnumerable<TSource>, TResult> _resultSelector;
    private readonly IEqualityComparer<TKey>? _comparer;

    public GroupBy3(TIterator source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
    {
        _keySelector = keySelector;
        _resultSelector = resultSelector;
        _comparer = comparer;
        _source = source;
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
        if (!_init) {
            _init = true;
            _lookupEnumerator = GetLookupEnumerator();
        }
        if (_lookupEnumerator!.MoveNext()) {
            current = _resultSelector(_lookupEnumerator.Current.Key, _lookupEnumerator.Current);
            return true;
        }
        current = default!;
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }

    private readonly IEnumerator<IGrouping<TKey, TSource>> GetLookupEnumerator()
    {
        return Lookup<TKey, TSource>.Create(_source, _keySelector, _comparer).GetEnumerator();
    }
}

public ref struct GroupBy4<TIterator, TSource, TKey, TElement, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private bool _init;
    private IEnumerator<IGrouping<TKey, TElement>>? _lookupEnumerator;
    private readonly Func<TSource, TKey> _keySelector;
    private readonly Func<TSource, TElement> _elementSelector;
    private readonly Func<TKey, IEnumerable<TElement>, TResult> _resultSelector;
    private readonly IEqualityComparer<TKey>? _comparer;

    public GroupBy4(TIterator source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
    {
        _keySelector = keySelector;
        _elementSelector = elementSelector;
        _resultSelector = resultSelector;
        _comparer = comparer;
        _source = source;
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
        if (!_init) {
            _init = true;
            _lookupEnumerator = GetLookupEnumerator();
        }
        if (_lookupEnumerator!.MoveNext()) {
            current = _resultSelector(_lookupEnumerator.Current.Key, _lookupEnumerator.Current);
            return true;
        }
        current = default!;
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }

    private readonly IEnumerator<IGrouping<TKey, TElement>> GetLookupEnumerator()
    {
        return Lookup<TKey, TElement>.Create(_source, _keySelector, _elementSelector, _comparer).GetEnumerator();
    }
}
