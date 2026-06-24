using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Index<TIterator, TSource>, (int Index, TSource Item)> Index<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator));
    }
}

public ref struct Index<TIterator, TSource> : IIterator<(int Index, TSource Item)>
        where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private int _index;

    public Index(TIterator source)
    {
        _source = source;
    }

    public readonly bool TryGetCount(out int count)
    {
        return _source.TryGetCount(out count);
    }

    public readonly bool TryGet(Index index, out (int Index, TSource Item) item)
    {
        if (_source.TryGetCount(out var count)) {
            if (_source.TryGet(index, out var value)) {
                item = (index.GetOffset(count), value);
                return true;
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out (int Index, TSource Item) current)
    {
        if (_source.TryGetNext(out var item)) {
            current = (_index++, item);
            return true;
        }
        current = default;
        return false;
    }

    public void Dispose() => _source.Dispose();
}
