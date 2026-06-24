using RvB.Linq.Utils;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<AggregateBy<TIterator, TSource, TKey, TAccumulate>, KeyValuePair<TKey, TAccumulate>> AggregateBy<TIterator, TSource, TKey, TAccumulate>(
            this Iterable<TIterator, TSource> source,
            Func<TSource, TKey> keySelector,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            IEqualityComparer<TKey>? keyComparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(func);
        return new(new(source._iterator, keySelector, seed, null, func, keyComparer));
    }

    public static Iterable<AggregateBy<TIterator, TSource, TKey, TAccumulate>, KeyValuePair<TKey, TAccumulate>> AggregateBy<TIterator, TSource, TKey, TAccumulate>(
            this Iterable<TIterator, TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TAccumulate> seedSelector,
            Func<TAccumulate, TSource, TAccumulate> func,
            IEqualityComparer<TKey>? keyComparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(seedSelector);
        ArgumentNullException.ThrowIfNull(func);
        return new(new(source._iterator, keySelector, default!, seedSelector, func, keyComparer));
    }
}

public ref struct AggregateBy<TIterator, TSource, TKey, TAccumulate> : IIterator<KeyValuePair<TKey, TAccumulate>>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, TKey> _keySelector;
    private readonly TAccumulate _seed;
    private readonly Func<TKey, TAccumulate>? _seedSelector;
    private readonly Func<TAccumulate, TSource, TAccumulate> _func;
    private readonly IEqualityComparer<TKey>? _keyComparer;

    private LightWeightDictionary<TKey, TAccumulate>? _map;
    private LightWeightDictionary<TKey, TAccumulate>.Enumerator _enumerator;

    public AggregateBy(
        TIterator source,
        Func<TSource, TKey> keySelector,
        TAccumulate seed,
        Func<TKey, TAccumulate>? seedSelector,
        Func<TAccumulate, TSource, TAccumulate> func,
        IEqualityComparer<TKey>? keyComparer)
    {
        _source = source;
        _keySelector = keySelector;
        _seed = seed;
        _seedSelector = seedSelector;
        _func = func;
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out KeyValuePair<TKey, TAccumulate> item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out KeyValuePair<TKey, TAccumulate> current)
    {
        if (_map is null) {
            _map = _source.TryGetCount(out var count) ? new(count, _keyComparer) : new(_keyComparer);
            while (_source.TryGetNext(out var item)) {
                var key = _keySelector(item);
                ref var accumulate = ref _map.GetValueRefOrAddDefault(key, out var exists);
                var seed = (_seedSelector is null) ? _seed : _seedSelector(key);
                accumulate = _func(exists ? accumulate! : seed, item);
            }
            _enumerator = _map.GetEnumerator();
        }
        if (_enumerator.MoveNext()) {
            var (key, value) = _enumerator.Current;
            current = new(key, value);
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        if (_map != null) {
            _enumerator.Dispose();
            _map.Clear();
            _map = null;
        }
        _source.Dispose();
    }
}
