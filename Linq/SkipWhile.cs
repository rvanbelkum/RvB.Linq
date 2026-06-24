using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<SkipWhile<TIterator, TSource>, TSource> SkipWhile<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, predicate, null));
    }

    public static Iterable<SkipWhile<TIterator, TSource>, TSource> SkipWhile<TIterator, TSource>(this Iterable<TIterator, TSource> source, Func<TSource, int, bool> predicate)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, null, predicate));
    }
}

public ref struct SkipWhile<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly Func<TSource, bool>? _predicate;
    private readonly Func<TSource, int, bool>? _predicateIndex;
    private int _state;
    private int _index;

    public SkipWhile(TIterator source, Func<TSource, bool>? predicate, Func<TSource, int, bool>? predicateIndex)
    {
        if (predicate is null && predicateIndex is null) {
            throw new NotImplementedException();
        }
        _source = source;
        _predicate = predicate;
        _predicateIndex = predicateIndex;
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
            while (_source.TryGetNext(out var item)) {
                if (!Predicate(item)) {
                    _state = 1;
                    current = item;
                    return true;
                }
            }
            _state = 2;
        }
        if (_state == 1) {
            if (_source.TryGetNext(out current)) {
                return true;
            }
            _state = 2;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    private bool Predicate(TSource current) => _predicate is not null ? _predicate(current) : _predicateIndex!(current, _index++);

    public void Dispose() => _source.Dispose();
}
