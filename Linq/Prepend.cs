using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Prepend<TIterator, TSource>, TSource> Prepend<TIterator, TSource>(this Iterable<TIterator, TSource> source, TSource prepend)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, prepend));
    }
}

public ref struct Prepend<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly TSource _prepend;
    private int _state1;

    public Prepend(TIterator source, TSource prepend)
    {
        _source = source;
        _prepend = prepend;
    }

    public bool TryGetCount(out int count)
    {
        if (_source.TryGetCount(out count)) {
            count += 1;
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_source.TryGetCount(out var count)) {
            if (index.IsFromEnd) {
                if (index.Value == count + 1) {
                    item = _prepend;
                    return true;
                } else if (index.Value <= count) {
                    return _source.TryGet(index, out item);
                }
            } else {
                if (index.Value == 0) {
                    item = _prepend;
                    return true;
                } else if (index.Value <= count) {
                    return _source.TryGet(index.Value - 1, out item);
                }
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_state1 == 0) {
            _state1 = 1;
            current = _prepend;
            return true;
        }
        return _source.TryGetNext(out current);
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}
