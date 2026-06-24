using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Union<TIterator, TIterator1, TSource>, TSource> Union<TIterator, TIterator1, TSource>(this Iterable<TIterator, TSource> source, Iterable<TIterator1, TSource> source1, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TIterator1 : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, source1._iterator, comparer));
    }
}

public ref struct Union<TIterator1, TIterator2, TSource> : IIterator<TSource>
    where TIterator1 : struct, IIterator<TSource>, allows ref struct
    where TIterator2 : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator1 _firstSource;
    private TIterator2 _secondSource;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly IEqualityComparer<TSource>? _comparer;
    private HashSet<TSource>? _visited;
    private int _state;

    public Union(TIterator1 firstSource, TIterator2 secondSource, IEqualityComparer<TSource>? comparer = null)
    {
        _firstSource = firstSource;
        _secondSource = secondSource;
        _comparer = comparer;
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
        InitVisited();
        if (_state == 0) {
            while (_firstSource.TryGetNext(out var item)) {
                if (_visited.Add(item)) {
                    current = item;
                    return true;
                }
            }
            _state = 1;
        }
        if (_state == 1) {
            while (_secondSource.TryGetNext(out var item)) {
                if (_visited.Add(item)) {
                    current = item;
                    return true;
                }
            }
            _state = 2;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _firstSource.Dispose();
        _secondSource.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [MemberNotNull(nameof(_visited))]
    private void InitVisited()
    {
        if (_visited != null) {
            return;
        }
        if (_firstSource.TryGetCount(out var count1) | _secondSource.TryGetCount(out var count2)) {
            // If one of the sources does not have a count, assume the total count is twice the count of the other
            var count = count1 == 0 ? count2 * 2 : count2 == 0 ? count1 * 2 : count1 + count2;
            _visited = new HashSet<TSource>(count, _comparer);
        } else {
            _visited = new(_comparer);
        }
    }
}
