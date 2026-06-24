using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class Iterable
{
    public static Iterable<Empty<TSource>, TSource> Empty<TSource>()
    {
        return new(default);
    }
}

public readonly ref struct Empty<TSource> : IIterator<TSource>
{
    public bool TryGetCount(out int count)
    {
        count = 0;
        return true;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() { }
}
