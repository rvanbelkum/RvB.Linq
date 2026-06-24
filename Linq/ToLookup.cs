using RvB.Linq.Utils;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static ILookup<TKey, TSource> ToLookup<TIterator, TSource, TKey>(
        this Iterable<TIterator, TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return Lookup<TKey, TSource>.Create(source._iterator, keySelector, comparer);
    }

    public static ILookup<TKey, TElement> ToLookup<TIterator, TSource, TKey, TElement>(
        this Iterable<TIterator, TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(elementSelector);
        return Lookup<TKey, TElement>.Create(source._iterator, keySelector, elementSelector, comparer);
    }
}

public class Lookup<TKey, TElement> : ILookup<TKey, TElement>, ICollection<IGrouping<TKey, TElement>>
{
    private readonly IEqualityComparer<TKey> _comparer;
    private LightWeightDictionary<TKey, Grouping<TKey, TElement>> _groupsByKey;

    private Lookup(IEqualityComparer<TKey>? comparer)
    {
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
        _groupsByKey = new(_comparer);
    }

    private void Add(TKey key, TElement element)
    {
        ref var grouping = ref _groupsByKey.GetValueRefOrAddDefault(key, out var exists);
        if (exists) {
            grouping!.Add(element);
        } else {
            grouping = new(key, element);
        }
    }

    internal static Lookup<TKey, TElement> Create<TIterator, TSource>(
        TIterator source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        Debug.Assert(keySelector is not null);
        Debug.Assert(elementSelector is not null);

        var lookup = new Lookup<TKey, TElement>(comparer);
        while (source.TryGetNext(out var item)) {
            lookup.Add(keySelector(item), elementSelector(item));
        }
        return lookup;
    }

    internal static Lookup<TKey, TElement> Create<TIterator>(
        TIterator source,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TElement>, allows ref struct
    {
        Debug.Assert(keySelector is not null);

        var lookup = new Lookup<TKey, TElement>(comparer);
        while (source.TryGetNext(out var item)) {
            lookup.Add(keySelector(item), item);
        }
        return lookup;
    }

    internal static Lookup<TKey, TElement> CreateForJoin<TIterator>(
        TIterator source,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer)
        where TIterator : struct, IIterator<TElement>, allows ref struct
    {
        Debug.Assert(keySelector is not null);

        var lookup = new Lookup<TKey, TElement>(comparer);
        while (source.TryGetNext(out var item)) {
            var key = keySelector(item);
            if (key != null) {
                lookup.Add(key, item);
            }
        }
        return lookup;
    }

    internal static Lookup<TKey, TElement> CreateForJoin(
        IEnumerable<TElement> source,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer)
    {
        Debug.Assert(keySelector is not null);

        var lookup = new Lookup<TKey, TElement>(comparer);
        foreach (var item in source) {
            var key = keySelector(item);
            if (key != null) {
                lookup.Add(key, item);
            }
        }
        return lookup;
    }

    public int Count => _groupsByKey.Count;

    bool ICollection<IGrouping<TKey, TElement>>.IsReadOnly => throw new NotImplementedException();

    public IEnumerable<TElement> this[TKey key] {
        get {
            if (_groupsByKey.TryGetValue(key, out var elements)) {
                return elements;
            }
            return [];
        }
    }

    public bool Contains(TKey key) => _groupsByKey.ContainsKey(key);

    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
    {
        foreach (var (_, grouping) in _groupsByKey) {
            yield return grouping;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal List<TResult> ToList<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
    {
        var list = new List<TResult>(_groupsByKey.Count); // list with capacity set internal buffer as source size
        CollectionsMarshal.SetCount(list, _groupsByKey.Count);
        var span = CollectionsMarshal.AsSpan(list);
        var i = 0;
        foreach (var (key, elements) in _groupsByKey) {
            span[i] = resultSelector(key, elements);
            i++;
        }
        return list;
    }

    public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(resultSelector);
        foreach (var (key, elements) in _groupsByKey) {
            yield return resultSelector(key, elements);
        }
    }

    internal bool TryGetGrouping(TKey key, [MaybeNullWhen(false)] out Grouping<TKey, TElement> grouping)
    {
        if (_groupsByKey.TryGetValue(key, out grouping)) {
            return true;
        }
        grouping = null;
        return false;
    }

    int ICollection<IGrouping<TKey, TElement>>.Count => Count;

    void ICollection<IGrouping<TKey, TElement>>.Add(IGrouping<TKey, TElement> item) => throw new NotImplementedException();

    void ICollection<IGrouping<TKey, TElement>>.Clear() => throw new NotImplementedException();

    bool ICollection<IGrouping<TKey, TElement>>.Contains(IGrouping<TKey, TElement> item)
    {
        if (_groupsByKey.TryGetValue(item.Key, out var grouping)) {
            if (grouping != null && grouping == item) {
                return true;
            }
        }
        return false;
    }

    void ICollection<IGrouping<TKey, TElement>>.CopyTo(IGrouping<TKey, TElement>[] array, int arrayIndex) => throw new NotImplementedException();
    bool ICollection<IGrouping<TKey, TElement>>.Remove(IGrouping<TKey, TElement> item) => throw new NotImplementedException();
}

[DebuggerDisplay("Key = {Key}")]
internal sealed class Grouping<TKey, TElement> :
    IGrouping<TKey, TElement>,
    IReadOnlyList<TElement>
{
    private readonly TKey _key;
    private List<TElement> _elements;

    internal Grouping(TKey key)
    {
        _key = key;
        _elements = [];
    }

    internal Grouping(TKey key, TElement element)
    {
        _key = key;
        _elements = [element];
    }

    internal Grouping(TKey key, IEnumerable<TElement> elements)
    {
        _key = key;
        _elements = elements.ToList();
    }

    public TKey Key => _key;

    public int Count => _elements.Count;

    public TElement this[int index] {
        get {
            if ((uint)index < (uint)_elements.Count) {
                return _elements[index];
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public void Add(TElement element)
    {
        _elements.Add(element);
    }

    public IEnumerator<TElement> GetEnumerator() => _elements.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    int IReadOnlyCollection<TElement>.Count => _elements.Count;

    TElement IReadOnlyList<TElement>.this[int index] {
        get {
            if ((uint)index < (uint)_elements.Count) {
                return _elements[index];
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
