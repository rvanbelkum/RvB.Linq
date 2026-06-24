using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<SelectMany<TIterator, TInnerIterator, TSource, TResult>, TResult> SelectMany<TIterator, TInnerIterator, TSource, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, Iterable<TInnerIterator, TResult>> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TInnerIterator : struct, IIterator<TResult>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator, selector));
    }

    public static Iterable<SelectManyIndex<TIterator, TInnerIterator, TSource, TResult>, TResult> SelectMany<TIterator, TInnerIterator, TSource, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, int, Iterable<TInnerIterator, TResult>> selector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TInnerIterator : struct, IIterator<TResult>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new(new(source._iterator, selector));
    }

    public static Iterable<SelectMany<TIterator, TInnerIterator, TSource, TCollection, TResult>, TResult> SelectMany<TIterator, TInnerIterator, TSource, TCollection, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, Iterable<TInnerIterator, TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TInnerIterator : struct, IIterator<TCollection>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, collectionSelector, resultSelector));
    }

    public static Iterable<SelectManyIndex<TIterator, TInnerIterator, TSource, TCollection, TResult>, TResult> SelectMany<TIterator, TInnerIterator, TSource, TCollection, TResult>(this Iterable<TIterator, TSource> source, Func<TSource, int, Iterable<TInnerIterator, TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        where TIterator : struct, IIterator<TSource>, allows ref struct
        where TInnerIterator : struct, IIterator<TCollection>, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return new(new(source._iterator, collectionSelector, resultSelector));
    }
}

public ref struct SelectMany<TIterator, TInnerIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TInnerIterator : struct, IIterator<TResult>, allows ref struct
{
    private TIterator _source;
    private TInnerIterator _innerIterator;
    private int _state;
    private readonly Func<TSource, Iterable<TInnerIterator, TResult>> _selector;

    public SelectMany(TIterator source, Func<TSource, Iterable<TInnerIterator, TResult>> selector)
    {
        _selector = selector;
        _source = source;
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
    BEGIN:
        if (_state == 0) {
            if (_source.TryGetNext(out var value)) {
                _innerIterator = _selector(value)._iterator;
                _state = 1;
            } else {
                _state = 2;
            }
        }
        if (_state == 1) {
            if (_innerIterator.TryGetNext(out current)) {
                return true;
            }
            _innerIterator.Dispose();
            _state = 0;
            goto BEGIN;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        if (_state == 1) {
            _innerIterator.Dispose();
        }
        _source.Dispose();
    }
}

public ref struct SelectManyIndex<TIterator, TInnerIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TInnerIterator : struct, IIterator<TResult>, allows ref struct
{
    private TIterator _source;
    private TInnerIterator _innerIterator;
    private int _state;
    private int _index = 0;
    private readonly Func<TSource, int, Iterable<TInnerIterator, TResult>> _selector;

    public SelectManyIndex(TIterator source, Func<TSource, int, Iterable<TInnerIterator, TResult>> selector)
    {
        _selector = selector;
        _source = source;
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
    BEGIN:
        if (_state == 0) {
            if (_source.TryGetNext(out var value)) {
                _innerIterator = _selector(value, _index++)._iterator;
                _state = 1;
            } else {
                _state = 2;
            }
        }
        if (_state == 1) {
            if (_innerIterator.TryGetNext(out current)) {
                return true;
            }
            _innerIterator.Dispose();
            _state = 0;
            goto BEGIN;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        if (_state == 1) {
            _innerIterator.Dispose();
        }
        _source.Dispose();
    }
}

public ref struct SelectMany<TIterator, TInnerIterator, TSource, TCollection, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TInnerIterator : struct, IIterator<TCollection>, allows ref struct
{
    private TIterator _source;
    private TInnerIterator _innerIterator;
    private TSource _currentSource = default!;
    private readonly Func<TSource, Iterable<TInnerIterator, TCollection>> _collectionSelector;
    private readonly Func<TSource, TCollection, TResult> _resultSelector;
    private int _state;

    public SelectMany(TIterator source, Func<TSource, Iterable<TInnerIterator, TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        _source = source;
        _collectionSelector = collectionSelector;
        _resultSelector = resultSelector;
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
    BEGIN:
        if (_state == 0) {
            if (_source.TryGetNext(out var value)) {
                _currentSource = value;
                _innerIterator = _collectionSelector(value)._iterator;
                _state = 1;
            } else {
                _state = 2;
            }
        }
        if (_state == 1) {
            if (_innerIterator.TryGetNext(out var innerCurrent)) {
                current = _resultSelector(_currentSource, innerCurrent);
                return true;
            }
            _innerIterator.Dispose();
            _state = 0;
            goto BEGIN;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        if (_state == 1) {
            _innerIterator.Dispose();
        }
        _source.Dispose();
    }
}

public ref struct SelectManyIndex<TIterator, TInnerIterator, TSource, TCollection, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TInnerIterator : struct, IIterator<TCollection>, allows ref struct
{
    private TIterator _source;
    private TInnerIterator _innerIterator;
    private TSource _currentSource = default!;
    private readonly Func<TSource, int, Iterable<TInnerIterator, TCollection>> _collectionSelector;
    private readonly Func<TSource, TCollection, TResult> _resultSelector;
    private int _state;
    private int _index;

    public SelectManyIndex(TIterator source, Func<TSource, int, Iterable<TInnerIterator, TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
    {
        _source = source;
        _collectionSelector = collectionSelector;
        _resultSelector = resultSelector;
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
    BEGIN:
        if (_state == 0) {
            if (_source.TryGetNext(out var value)) {
                _currentSource = value;
                _innerIterator = _collectionSelector(value, _index++)._iterator;
                _state = 1;
            } else {
                _state = 2;
            }
        }
        if (_state == 1) {
            if (_innerIterator.TryGetNext(out var innerCurrent)) {
                current = _resultSelector(_currentSource, innerCurrent);
                return true;
            }
            _innerIterator.Dispose();
            _state = 0;
            goto BEGIN;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        if (_state == 1) {
            _innerIterator.Dispose();
        }
        _source.Dispose();
    }
}
