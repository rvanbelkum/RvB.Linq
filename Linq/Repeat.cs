using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class Iterable
{
    public static Iterable<Repeat<TSource>, TSource> Repeat<TSource>(TSource element, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        return new(new(element, count));
    }
}

public ref struct Repeat<TSource> : IIterator<TSource>
{
    private readonly TSource _element;
    private int _count;

    public Repeat(TSource element, int count)
    {
        _element = element;
        _count = count;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = _count;
        return true;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        item = _element;
        return true;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_count > 0) {
            _count -= 1;
            current = _element;
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() { }
}
