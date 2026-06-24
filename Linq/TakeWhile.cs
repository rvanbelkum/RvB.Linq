using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<TakeWhile<TIterator, TSource>, TSource> TakeWhile<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator, predicate));
    }

    public static Iterable<TakeWhileIndex<TIterator, TSource>, TSource> TakeWhile<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, int, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator, predicate));
    }
}

public ref struct TakeWhile<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, bool> _predicate;
    private int _state;

    public TakeWhile(TIterator source, Func<TSource, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
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
            if (_source.TryGetNext(out current) && _predicate(current)) {
                return true;
            }
            _state = 1;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}

public ref struct TakeWhileIndex<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, int, bool> _predicate;
    private int _state;
    private int _index;

    public TakeWhileIndex(TIterator source, Func<TSource, int, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
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
            if (_source.TryGetNext(out current) && _predicate(current, _index++)) {
                return true;
            }
            _state = 1;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}
