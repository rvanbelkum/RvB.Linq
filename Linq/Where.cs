using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Where<TIterator, TSource>, TSource> Where<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator, predicate));
    }

    public static Iterable<WhereIndex<TIterator, TSource>, TSource> Where<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, int, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator, predicate));
    }

    public static Iterable<SelectWhere<TIterator, TSource, TResult>, TResult> Where<TIterator, TSource, TResult>(this Iterable<Select<TIterator, TSource, TResult>, TResult> source, Func<TResult, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator._source, source._iterator._selector, predicate));
    }

    public static Iterable<SelectIndexWhere<TIterator, TSource, TResult>, TResult> Where<TIterator, TSource, TResult>(this Iterable<SelectIndex<TIterator, TSource, TResult>, TResult> source, Func<TResult, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator._source, source._iterator._selector, predicate));
    }

    public static Iterable<SelectWhereIndex<TIterator, TSource, TResult>, TResult> Where<TIterator, TSource, TResult>(this Iterable<Select<TIterator, TSource, TResult>, TResult> source, Func<TResult, int, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator._source, source._iterator._selector, predicate));
    }

    public static Iterable<SelectIndexWhereIndex<TIterator, TSource, TResult>, TResult> Where<TIterator, TSource, TResult>(this Iterable<SelectIndex<TIterator, TSource, TResult>, TResult> source, Func<TResult, int, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(new(source._iterator._source, source._iterator._selector, predicate));
    }
}

public ref struct Where<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal readonly Func<TSource, bool> _predicate;

    public Where(TIterator source, Func<TSource, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    public bool TryGetCount(out int count)
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
        while (_source.TryGetNext(out var value)) {
            if (_predicate(value)) {
                current = value;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}

public ref struct WhereIndex<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal readonly Func<TSource, int, bool> _predicate;
    private int _index = 0;

    public WhereIndex(TIterator source, Func<TSource, int, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = default;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        while (_source.TryGetNext(out var value)) {
            if (_predicate(value, _index++)) {
                current = value;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}

public ref struct WhereSelect<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TSource, bool> _predicate;
    internal Func<TSource, TResult> _selector;

    public WhereSelect(TIterator source, Func<TSource, bool> predicate, Func<TSource, TResult> selector)
    {
        _source = source;
        _predicate = predicate;
        _selector = selector;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TResult item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        while (_source.TryGetNext(out var value)) {
            if (_predicate(value)) {
                current = _selector(value);
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

public ref struct WhereIndexSelect<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TSource, int, bool> _predicate;
    internal Func<TSource, TResult> _selector;
    private int _index = 0;

    public WhereIndexSelect(TIterator source, Func<TSource, int, bool> predicate, Func<TSource, TResult> selector)
    {
        _source = source;
        _predicate = predicate;
        _selector = selector;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TResult item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        while (_source.TryGetNext(out var value)) {
            if (_predicate(value, _index++)) {
                current = _selector(value);
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}

public ref struct WhereSelectIndex<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TSource, bool> _predicate;
    internal Func<TSource, int, TResult> _selector;
    private int _index = 0;

    public WhereSelectIndex(TIterator source, Func<TSource, bool> predicate, Func<TSource, int, TResult> selector)
    {
        _source = source;
        _predicate = predicate;
        _selector = selector;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TResult item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        while (_source.TryGetNext(out var value)) {
            if (_predicate(value)) {
                current = _selector(value, _index++);
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}

public ref struct WhereIndexSelectIndex<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TSource, int, bool> _predicate;
    internal Func<TSource, int, TResult> _selector;
    private int _whereIndex = 0;
    private int _selectIndex = 0;

    public WhereIndexSelectIndex(TIterator source, Func<TSource, int, bool> predicate, Func<TSource, int, TResult> selector)
    {
        _source = source;
        _predicate = predicate;
        _selector = selector;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TResult item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        while (_source.TryGetNext(out var value)) {
            if (_predicate(value, _whereIndex++)) {
                current = _selector(value, _selectIndex++);
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}
