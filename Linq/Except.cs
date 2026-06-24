using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Except<TIterator, TIterator2, TSource>, TSource> Except<TIterator, TIterator2, TSource>(this Iterable<TIterator, TSource> source, Iterable<TIterator2, TSource> second, IEqualityComparer<TSource>? comparer = null)
         where TIterator : struct, IIterator<TSource>, allows ref struct
         where TIterator2 : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, second, comparer));
    }

    public static Iterable<Except<TIterator, EnumerableIterator<TSource>, TSource>, TSource> Except<TIterator, TSource>(this Iterable<TIterator, TSource> source, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(second);
        return new(new(source._iterator, second.AsIterable(), comparer));
    }
}

public ref struct Except<TIterator, TIterator2, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TIterator2 : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
    private Iterable<TIterator2, TSource> _second;
    private IEqualityComparer<TSource>? _comparer;
#pragma warning restore IDE0044 // Add readonly modifier
    private HashSet<TSource>? _secondSet;

    public Except(TIterator source, Iterable<TIterator2, TSource> second, IEqualityComparer<TSource>? comparer)
    {
        _source = source;
        _second = second;
        _comparer = comparer ?? EqualityComparer<TSource>.Default;
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
        _secondSet ??= _second.ToHashSet(_comparer);
        while (_source.TryGetNext(out var value)) {
            if (_secondSet.Add(value)) {
                current = value;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}
