using RvB.Linq.Utils;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Chunk<TIterator, TSource>, TSource[]> Chunk<TIterator, TSource>(this Iterable<TIterator, TSource> source, int chunkSize)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize, nameof(chunkSize));
        chunkSize = Math.Min(chunkSize, Array.MaxLength);
        return new(new(source._iterator, chunkSize));
    }
}

public ref struct Chunk<TIterator, TSource> : IIterator<TSource[]>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly int _chunkSize;

    public Chunk(TIterator iterator, int chunkSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_chunkSize, nameof(chunkSize));
        _source = iterator;
        _chunkSize = chunkSize;
    }

    public readonly bool TryGetCount(out int count)
    {
        if (_source.TryGetCount(out count)) {
            count = (count + _chunkSize - 1) / _chunkSize;
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource[] item)
    {
        if (_source.TryGetCount(out var count)) {
            var offset = index.GetOffset(count);
            var start = offset * _chunkSize;
            var size = int.Min(_chunkSize, count - start);
            if (_source.TryGet(start, out var value)) {
                var chunk = new TSource[size];
                var i = 0;
                do {
                    chunk[i++] = value;
                } while (i < size && _source.TryGet(start + i, out value));
                item = chunk;
                return true;
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource[] current)
    {
        if (_source.TryGetNext(out var item)) {
            var index = 0;
            using (var arrayBuilder = new ArrayBuilder<TSource>()) {
                do {
                    arrayBuilder.Add(item);
                    index += 1;
                } while (index < _chunkSize && _source.TryGetNext(out item));
                current = arrayBuilder.ToArray();
            }
            return true;
        }
        current = default!;
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}
