using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<UnionBy<TIterator, TIterator2, TSource, TKey>, TSource> UnionBy<TIterator, TIterator2, TSource, TKey>(this Iterable<TIterator, TSource> source, Iterable<TIterator2, TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TIterator2 : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return new(new(source._iterator, second._iterator, keySelector, comparer));
    }
}

public ref struct UnionBy<TIterator, TIterator2, TSource, TKey> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TIterator2 : struct, IIterator<TSource>, allows ref struct
{
    private TIterator _source;
    private TIterator2 _second;
    private HashSet<TKey>? _set;
    private byte _state = 0;
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IEqualityComparer<TKey> _comparer;

    public UnionBy(TIterator source, TIterator2 second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
    {
        _keySelector = keySelector;
        _comparer = comparer ?? EqualityComparer<TKey>.Default;
        _source = source;
        _second = second;
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
        if (_state == 0) {
            if (_source.TryGetCount(out var count1) | _second.TryGetCount(out var count2)) {
                _set = new(count1 + count2, _comparer);
            } else {
                _set = new(_comparer);
            }
            _state = 1;
        }
        if (_state == 1) {
            while (_source.TryGetNext(out var value)) {
                if (_set!.Add(_keySelector(value))) {
                    current = value;
                    return true;
                }
            }
            _state = 2;
        }
        if (_state == 2) {
            while (_second.TryGetNext(out var value)) {
                if (_set!.Add(_keySelector(value))) {
                    current = value;
                    return true;
                }
            }
            _state = 3;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _state = 3;
        _set?.Clear();
        _source.Dispose();
        _second.Dispose();
    }
}
