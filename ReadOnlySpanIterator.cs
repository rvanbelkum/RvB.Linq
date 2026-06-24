using System.Runtime.CompilerServices;

namespace RvB.Linq;

public ref struct ReadOnlySpanIterator<T> : IIterator<T>
{
    private readonly ReadOnlySpan<T> _source;
    private int _index;

    public ReadOnlySpanIterator(ReadOnlySpan<T> source)
    {
        _source = source;
        _index = 0;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = _source.Length;
        return true;
    }

    public readonly bool TryGet(Index index, out T item)
    {
        var offset = index.GetOffset(_source.Length);
        if ((uint)offset < (uint)_source.Length) {
            item = _source[index];
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out T current)
    {
        if ((uint)_index < (uint)_source.Length) {
            current = _source[_index++];
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public readonly void Dispose() { }
}
