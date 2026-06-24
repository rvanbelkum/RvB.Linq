using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<DefaultIfEmpty<TIterator, TSource>, TSource> DefaultIfEmpty<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator));
    }

    public static Iterable<DefaultIfEmpty<TIterator, TSource>, TSource> DefaultIfEmpty<TIterator, TSource>(this Iterable<TIterator, TSource> source, TSource defaultValue)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, defaultValue));
    }
}

public ref struct DefaultIfEmpty<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _iterator;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly TSource _defaultValue;
    private int _state;

    public DefaultIfEmpty(TIterator iterator) : this(iterator, default!) { }

    public DefaultIfEmpty(TIterator iterator, TSource defaultValue)
    {
        _iterator = iterator;
        _defaultValue = defaultValue;
    }

    public readonly bool TryGetCount(out int count)
    {
        if (_iterator.TryGetCount(out count)) {
            if (count == 0) {
                count = 1;
            }
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_iterator.TryGetCount(out var count)) {
            if (count == 0) {
                item = default!;
                return true;
            }
            return _iterator.TryGet(index, out item);
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_state == 0) {
            if (_iterator.TryGetNext(out current)) {
                _state = 1;
                return true;
            }
            current = _defaultValue;
            _state = 2;
            return true;
        }
        if (_state == 1) {
            if (_iterator.TryGetNext(out current)) {
                return true;
            }
            _state = 2;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _iterator.Dispose();
    }
}
