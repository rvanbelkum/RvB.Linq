using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Zip<TIterator, TIterator2, TFirst, TSecond>, (TFirst First, TSecond Second)> Zip<TIterator, TIterator2, TFirst, TSecond>(this Iterable<TIterator, TFirst> source, Iterable<TIterator2, TSecond> second)
        where TIterator : struct, IIterator<TFirst>, allows ref struct
        where TIterator2 : struct, IIterator<TSecond>, allows ref struct
    {
        return new(new(source._iterator, second._iterator));
    }

    public static Iterable<Zip<TIterator, TIterator2, TIterator3, TFirst, TSecond, TThird>, (TFirst First, TSecond Second, TThird Third)> Zip<TIterator, TIterator2, TIterator3, TFirst, TSecond, TThird>(this Iterable<TIterator, TFirst> source, Iterable<TIterator2, TSecond> second, Iterable<TIterator3, TThird> third)
        where TIterator : struct, IIterator<TFirst>, allows ref struct
        where TIterator2 : struct, IIterator<TSecond>, allows ref struct
        where TIterator3 : struct, IIterator<TThird>, allows ref struct
    {
        return new(new(source._iterator, second._iterator, third._iterator));
    }

    public static Iterable<Zip<TIterator, TIterator2, TFirst, TSecond, TResult>, TResult> Zip<TIterator, TIterator2, TFirst, TSecond, TResult>(this Iterable<TIterator, TFirst> source, Iterable<TIterator2, TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        where TIterator : struct, IIterator<TFirst>, allows ref struct
        where TIterator2 : struct, IIterator<TSecond>, allows ref struct
    {
        return new(new(source._iterator, second._iterator, resultSelector));
    }
}

public ref struct Zip<TIterator, TIterator2, TFirst, TSecond> : IIterator<(TFirst First, TSecond Second)>
    where TIterator : struct, IIterator<TFirst>, allows ref struct
    where TIterator2 : struct, IIterator<TSecond>, allows ref struct
{
    private TIterator _first;
    private TIterator2 _second;

    public Zip(TIterator first, TIterator2 second)
    {
        _first = first;
        _second = second;
    }

    public bool TryGetCount(out int count)
    {
        if (_first.TryGetCount(out var count1) && _second.TryGetCount(out var count2)) {
            count = int.Min(count1, count2);
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out (TFirst First, TSecond Second) item)
    {
        if (_first.TryGet(index, out var first) && _second.TryGet(index, out var second)) {
            item = (first, second);
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out (TFirst First, TSecond Second) current)
    {
        if (_first.TryGetNext(out var first) && _second.TryGetNext(out var second)) {
            current = (first, second);
            return true;
        }
        current = default;
        return false;
    }

    public void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
    }
}

public ref struct Zip<TIterator, TIterator2, TIterator3, TFirst, TSecond, TThird> : IIterator<(TFirst First, TSecond Second, TThird Third)>
    where TIterator : struct, IIterator<TFirst>, allows ref struct
    where TIterator2 : struct, IIterator<TSecond>, allows ref struct
    where TIterator3 : struct, IIterator<TThird>, allows ref struct
{
    private TIterator _first;
    private TIterator2 _second;
    private TIterator3 _third;

    public Zip(TIterator first, TIterator2 second, TIterator3 third)
    {
        _first = first;
        _second = second;
        _third = third;
    }

    public bool TryGetCount(out int count)
    {
        if (_first.TryGetCount(out var count1) && _second.TryGetCount(out var count2) && _third.TryGetCount(out var count3)) {
            count = int.Min(count1, int.Min(count2, count3));
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out (TFirst First, TSecond Second, TThird Third) item)
    {
        if (_first.TryGet(index, out var first) && _second.TryGet(index, out var second) && _third.TryGet(index, out var third)) {
            item = (first, second, third);
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out (TFirst First, TSecond Second, TThird Third) current)
    {
        if (_first.TryGetNext(out var first) && _second.TryGetNext(out var second) && _third.TryGetNext(out var third)) {
            current = (first, second, third);
            return true;
        }
        current = default;
        return false;
    }

    public void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
        _third.Dispose();
    }
}

public ref struct Zip<TIterator, TIterator2, TFirst, TSecond, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TFirst>, allows ref struct
    where TIterator2 : struct, IIterator<TSecond>, allows ref struct
{
    private TIterator _first;
    private TIterator2 _second;
    private readonly Func<TFirst, TSecond, TResult> _resultSelector;

    public Zip(TIterator first, TIterator2 second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        _first = first;
        _second = second;
        _resultSelector = resultSelector;
    }

    public bool TryGetCount(out int count)
    {
        if (_first.TryGetCount(out var count1) && _second.TryGetCount(out var count2)) {
            count = int.Min(count1, count2);
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TResult item)
    {
        if (_first.TryGet(index, out var first) && _second.TryGet(index, out var second)) {
            item = _resultSelector(first, second);
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        if (_first.TryGetNext(out var first) && _second.TryGetNext(out var second)) {
            current = _resultSelector(first, second);
            return true;
        }
        current = default!;
        return false;
    }

    public void Dispose()
    {
        _first.Dispose();
        _second.Dispose();
    }
}
