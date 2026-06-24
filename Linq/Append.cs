using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Append<TIterator, TSource>, TSource> Append<TIterator, TSource>(this Iterable<TIterator, TSource> source, TSource append)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, append));
    }
}

public ref struct Append<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly TSource _append;
    private bool _finished;

    public Append(TIterator source, TSource append)
    {
        _source = source;
        _append = append;
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
                if (index.Value == 1) {
                    item = _append;
                    return true;
                } else if (index.Value - 1 <= count) {
                    return _source.TryGet(new(index.Value - 1, fromEnd: true), out item);
                }
            } else {
                if (index.Value == count) {
                    item = _append;
                    return true;
                } else if (index.Value < count) {
                    return _source.TryGet(index, out item);
                }
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public TSource Get(int index)
    {
        if (_source.TryGet(index, out var item)) {
            return item;
        }
        throw new NotSupportedException();
    }

    public bool TryGetNext(out TSource current)
    {
        if (_finished) {
            Unsafe.SkipInit(out current);
            return false;
        }
        if (_source.TryGetNext(out current)) {
            return true;
        }
        _finished = true;
        current = _append;
        return true;
    }

    public void Dispose()
    {
        _finished = true;
        _source.Dispose();
    }
}
