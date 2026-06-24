using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Reverse<TIterator, TSource>, TSource> Reverse<TIterator, TSource>(this Iterable<TIterator, TSource> source)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator));
    }
}

public ref struct Reverse<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private int _state;
    private TSource[]? _buffer;
    private int _index;

    public Reverse(TIterator source)
    {
        _source = source;
    }

    public readonly bool TryGetCount(out int count)
    {
        return _source.TryGetCount(out count);
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_source.TryGetCount(out var count)) {
            var offset = index.GetOffset(count);
            return _source.TryGet(count - 1 - offset, out item);
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_state == 0) {
            _buffer = IteratorExtensions.ToArray<TIterator, TSource>(ref _source);
            _state = 1;
            _index = _buffer.Length;
        }
        if (_state == 1) {
            if (_index > 0) {
                current = _buffer![--_index];
                return true;
            }
            _state = 2;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();
}
