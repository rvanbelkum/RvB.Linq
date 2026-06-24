using RvB.Linq.Utils;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<OrderBy<TIterator, TSource, TKey>, TSource> OrderBy<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, keySelector, descending: false, comparer));
    }

    public static Iterable<OrderBy<TIterator, TSource, TKey>, TSource> OrderByDescending<TIterator, TSource, TKey>(this Iterable<TIterator, TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, keySelector, descending: true, comparer));
    }

    public static Iterable<OrderBy<TIterator, TSource, TSecondKey>, TSource> ThenBy<TIterator, TSource, TKey, TSecondKey>(this Iterable<OrderBy<TIterator, TSource, TKey>, TSource> source, Func<TSource, TSecondKey> keySelector, IComparer<TSecondKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return new(source._iterator.ThenBy(keySelector, comparer));
    }

    public static Iterable<OrderBy<TIterator, TSource, TSecondKey>, TSource> ThenByDescending<TIterator, TSource, TKey, TSecondKey>(this Iterable<OrderBy<TIterator, TSource, TKey>, TSource> source, Func<TSource, TSecondKey> keySelector, IComparer<TSecondKey>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(keySelector);
        return new(source._iterator.ThenByDescending(keySelector, comparer));
    }
}

public ref struct OrderBy<TIterator, TSource, TKey> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private TSource[]? _sorted;
    private int _index;

    // _comparers[0] will always be the MainComparer. The rest (if any) will be ChildComparers
    private readonly IOrderByComparer<TSource>[] _comparers;

    public OrderBy(TIterator source, Func<TSource, TKey> keySelector, bool descending, IComparer<TKey>? comparer)
    {
        _source = source;
        comparer ??= Comparer<TKey>.Default;
        var mainComparer = new MainComparer<TSource, TKey>(keySelector, comparer, descending);
        _comparers = [mainComparer];
    }

    // Only used by ThenBy and ThenByDescending methods
    private OrderBy(TIterator source, Func<TSource, TKey> keySelector, bool descending, IComparer<TKey>? comparer, IOrderByComparer<TSource>[] parentComparers)
    {
        _source = source;
        comparer ??= Comparer<TKey>.Default;
        var childComparer = new ChildComparer<TSource, TKey>(keySelector, comparer, descending);
        _comparers = [.. parentComparers, childComparer];
    }

    public bool TryGetCount(out int count)
    {
        return _source.TryGetCount(out count);
    }

    public bool TryGet(Index index, out TSource item)
    {
        Sort();
        var offset = index.GetOffset(_sorted.Length);
        if ((uint)offset < (uint)_sorted.Length) {
            item = _sorted[index];
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        Sort();
        if (_index < _sorted.Length) {
            current = _sorted[_index++];
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public readonly OrderBy<TIterator, TSource, TSecondKey> ThenBy<TSecondKey>(Func<TSource, TSecondKey> keySelector, IComparer<TSecondKey>? comparer = null)
    {
        return new(_source, keySelector, descending: false, comparer, _comparers);
    }

    public readonly OrderBy<TIterator, TSource, TSecondKey> ThenByDescending<TSecondKey>(Func<TSource, TSecondKey> keySelector, IComparer<TSecondKey>? comparer = null)
    {
        return new(_source, keySelector, descending: true, comparer, _comparers!);
    }

    public void Dispose()
    {
        _source.Dispose();
    }

    [MemberNotNull(nameof(_sorted))]
    private void Sort()
    {
        if (_sorted != null) {
            return;
        }
        _sorted = IteratorExtensions.ToArray<TIterator, TSource>(ref _source);
        Sort(_sorted.AsSpan());
        _index = 0;
    }

    private readonly void Sort(Span<TSource> data)
    {
        // _comparers[0] must be the MainComparer. The rest (if any) are ChildComparers
        var mainComparer = _comparers[0] as IOrderBySource<TSource>;
        mainComparer!.SetSource(data, _comparers[1..]);

        long size = sizeof(int) * (long)data.Length;
        if (size <= 1024) {
            Span<int> indexMapSpan = stackalloc int[data.Length];
            SimdExtensions.FillIncrementing(indexMapSpan, 0);
            indexMapSpan.Sort(data, mainComparer as IComparer<int>);
        } else {
            using (var indexMap = new PooledArray<int>(data.Length)) {
                var indexMapSpan = indexMap.AsSpan();
                SimdExtensions.FillIncrementing(indexMapSpan, 0);
                indexMapSpan.Sort(data, mainComparer as IComparer<int>);
            }
        }
        mainComparer.Dispose();
    }
}

internal interface IOrderByComparer<TSource>
{
    int Compare(TSource x, TSource y);
}

internal interface IOrderBySource<TSource> : IDisposable
{
    void SetSource(ReadOnlySpan<TSource> source, IOrderByComparer<TSource>[] childComparers);
}

internal sealed class ChildComparer<TSource, TKey> : IOrderByComparer<TSource>
{
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IComparer<TKey> _comparer;
    private readonly bool _descending;

    public ChildComparer(Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool descending)
    {
        _keySelector = keySelector;
        _comparer = comparer;
        _descending = descending;
    }

    public int Compare(TSource x, TSource y)
    {
        var result = _comparer.Compare(_keySelector(x), _keySelector(y));
        if (_descending) {
            return -int.Sign(result);
        } else {
            return int.Sign(result);
        }
    }
}

internal sealed class MainComparer<TSource, TKey> : IOrderBySource<TSource>, IOrderByComparer<TSource>, IComparer<int>
{
    private TSource[]? _source;
    private readonly Func<TSource, TKey> _keySelector;
    private readonly IComparer<TKey> _comparer;
    private readonly bool _descending;
    private IOrderByComparer<TSource>[]? _childComparers;

    public MainComparer(Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool descending)
    {
        _keySelector = keySelector;
        _comparer = comparer;
        _descending = descending;
    }

    public void SetSource(ReadOnlySpan<TSource> source, IOrderByComparer<TSource>[] childComparers)
    {
        _source = ArrayPool<TSource>.Shared.Rent(source.Length);
        source.CopyTo(_source);
        _childComparers = childComparers;
    }

    int IComparer<int>.Compare(int x, int y)
    {
        ref var source1 = ref _source![x];
        ref var source2 = ref _source![y];
        var result = _comparer.Compare(_keySelector(source1), _keySelector(source2));
        if (result == 0) {
            foreach (var childComparer in _childComparers!) {
                result = childComparer.Compare(source1, source2);
                if (result != 0) {
                    return result;
                }
            }
            if (x == y) {
                return 0;
            }
            return (x < y) ? -1 : 1;
        }
        if (_descending) {
            return -int.Sign(result);
        } else {
            return int.Sign(result);
        }
    }

    public void Dispose()
    {
        if (_source != null) {
            ArrayPool<TSource>.Shared.Return(_source);
            _source = null;
        }
    }

    int IOrderByComparer<TSource>.Compare(TSource x, TSource y) => throw new NotImplementedException();
}
