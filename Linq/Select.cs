using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Select<TIterator, TSource, TResult>, TResult> Select<TIterator, TSource, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator, selector));
    }

    public static Iterable<SelectIndex<TIterator, TSource, TResult>, TResult> Select<TIterator, TSource, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, int, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator, selector));
    }

    public static Iterable<WhereSelect<TIterator, TSource, TResult>, TResult> Select<TIterator, TSource, TResult>(this Iterable<Where<TIterator, TSource>, TSource> source, Func<TSource, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator._source, source._iterator._predicate, selector));
    }

    public static Iterable<WhereIndexSelect<TIterator, TSource, TResult>, TResult> Select<TIterator, TSource, TResult>(this Iterable<WhereIndex<TIterator, TSource>, TSource> source, Func<TSource, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator._source, source._iterator._predicate, selector));
    }

    public static Iterable<WhereSelectIndex<TIterator, TSource, TResult>, TResult> Select<TIterator, TSource, TResult>(this Iterable<Where<TIterator, TSource>, TSource> source, Func<TSource, int, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator._source, source._iterator._predicate, selector));
    }

    public static Iterable<WhereIndexSelectIndex<TIterator, TSource, TResult>, TResult> Select<TIterator, TSource, TResult>(this Iterable<WhereIndex<TIterator, TSource>, TSource> source, Func<TSource, int, TResult> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator._source, source._iterator._predicate, selector));
    }
}

public ref struct Select<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal readonly Func<TSource, TResult> _selector;

    public Select(TIterator source, Func<TSource, TResult> selector)
    {
        _source = source;
        _selector = selector;
    }

    public bool TryGetCount(out int count)
        => _source.TryGetCount(out count);

    public readonly bool TryGet(Index index, out TResult item)
    {
        if (_source.TryGet(index, out var value)) {
            item = _selector(value);
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        if (_source.TryGetNext(out var value)) {
            current = _selector(value);
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}

public ref struct SelectIndex<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal readonly Func<TSource, int, TResult> _selector;
    private int? _count;
    private int _index = 0;

    public SelectIndex(TIterator source, Func<TSource, int, TResult> selector)
    {
        _source = source;
        _selector = selector;
        if (_source.TryGetCount(out var count)) {
            _count = count;
        }
    }

    public bool TryGetCount(out int count)
    {
        if (_count.HasValue) {
            count = _count.Value;
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TResult item)
    {
        if (index.IsFromEnd) {
            if (_count.HasValue) {
                if (_source.TryGet(index, out var value)) {
                    item = _selector(value, index.GetOffset(_count.Value));
                    return true;
                }
            }
        } else {
            if (_source.TryGet(index, out var value)) {
                item = _selector(value, index.Value);
                return true;
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        if (_source.TryGetNext(out var value)) {
            current = _selector(value, _index++);
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}

public ref struct SelectWhere<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TResult, bool> _predicate;
    internal Func<TSource, TResult> _selector;

    public SelectWhere(TIterator source, Func<TSource, TResult> selector, Func<TResult, bool> predicate)
    {
        _source = source;
        _selector = selector;
        _predicate = predicate;
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
            var selectValue = _selector(value);
            if (_predicate(selectValue)) {
                current = selectValue;
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

public ref struct SelectIndexWhere<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TResult, bool> _predicate;
    internal Func<TSource, int, TResult> _selector;
    private int _index;

    public SelectIndexWhere(TIterator source, Func<TSource, int, TResult> selector, Func<TResult, bool> predicate)
    {
        _source = source;
        _selector = selector;
        _predicate = predicate;
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
            var selectValue = _selector(value, _index++);
            if (_predicate(selectValue)) {
                current = selectValue;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() { _source.Dispose(); }
}

public ref struct SelectWhereIndex<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TResult, int, bool> _predicate;
    internal Func<TSource, TResult> _selector;
    private int _index;

    public SelectWhereIndex(TIterator source, Func<TSource, TResult> selector, Func<TResult, int, bool> predicate)
    {
        _source = source;
        _selector = selector;
        _predicate = predicate;
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
            var selectValue = _selector(value);
            if (_predicate(selectValue, _index++)) {
                current = selectValue;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() { _source.Dispose(); }
}

public ref struct SelectIndexWhereIndex<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    internal TIterator GetSource() => _source;
    internal Func<TResult, int, bool> _predicate;
    internal Func<TSource, int, TResult> _selector;
    private int _selectIndex;
    private int _whereIndex;

    public SelectIndexWhereIndex(TIterator source, Func<TSource, int, TResult> selector, Func<TResult, int, bool> predicate)
    {
        _source = source;
        _selector = selector;
        _predicate = predicate;
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
            var selectValue = _selector(value, _selectIndex++);
            if (_predicate(selectValue, _whereIndex++)) {
                current = selectValue;
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() { _source.Dispose(); }
}
