using RvB.Linq.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<CountBy<TIterator, TSource, TKey>, KeyValuePair<TKey, int>> CountBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, keySelector));
    }

    public static Iterable<CountBy<TIterator, TSource, TKey>, KeyValuePair<TKey, int>> CountBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, keySelector, keyComparer));
    }
}

public ref struct CountBy<TIterator, TSource, TKey> : IIterator<KeyValuePair<TKey, int>>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _iterator;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IEqualityComparer<TKey>? _keyComparer;
    private LightWeightDictionary<TKey, int>? _countsBy;
    private LightWeightDictionary<TKey, int>.Enumerator _countsByEnumerator;

    public CountBy(TIterator iterator, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null)
    {
        _iterator = iterator;
        _keySelector = keySelector;
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out KeyValuePair<TKey, int> item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out KeyValuePair<TKey, int> current)
    {
        if (_countsBy == null) {
            Initialize();
        }
        if (_countsByEnumerator.MoveNext()) {
            var (key, value) = _countsByEnumerator.Current;
            current = new(key, value);
            return true;
        }
        current = default;
        return false;
    }

    public void Dispose()
    {
        if (_countsBy != null) {
            _countsByEnumerator.Dispose();
        }
        _iterator.Dispose();
    }

    [MemberNotNull(nameof(_countsBy))]
    private void Initialize()
    {
        if (_countsBy is not null) {
            return;
        }
        _countsBy = new(_keyComparer);
        while (_iterator.TryGetNext(out var item)) {
            var key = _keySelector(item);
            ref int currentCount = ref _countsBy.GetValueRefOrAddDefault(key, out _);
            checked { currentCount++; }
        }
        _countsByEnumerator = _countsBy.GetEnumerator();
    }
}
