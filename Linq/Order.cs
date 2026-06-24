using RvB.Linq.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Order<TIterator, TSource>, TSource> Order<TIterator, TSource>(this Iterable<TIterator, TSource> source, IComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, false, comparer));
    }

    public static Iterable<Order<TIterator, TSource>, TSource> OrderDescending<TIterator, TSource>(this Iterable<TIterator, TSource> source, IComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, true, comparer));
    }
}

internal class SafeComparer<T> : IComparer<T> where T : allows ref struct
{
    private IComparer<T> _inner;
    private readonly bool _descending;

    public SafeComparer(IComparer<T> inner, bool descending)
    {
        _inner = inner;
        _descending = descending;
    }

    public int Compare(T? x, T? y)
    {
        if (_descending) {
            return int.Sign(_inner.Compare(y, x));
        }
        return int.Sign(_inner.Compare(x, y));
    }
}

public ref struct Order<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly IComparer<TSource> _comparer;
    private TSource[]? _sorted;
    private int _index;

    public Order(TIterator source, bool descending, IComparer<TSource>? comparer)
    {
        _source = source;
        if (comparer is null) {
            _comparer = (descending ? DescendingComparer<TSource>.Default : Comparer<TSource>.Default);
        } else {
            _comparer = new SafeComparer<TSource>(comparer, descending);
        }
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
            item = _sorted[offset];
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
        _sorted.AsSpan().Sort(_comparer);
        _index = 0;
    }
}
