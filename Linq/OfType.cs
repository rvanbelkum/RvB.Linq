using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
}

public ref struct OfType<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier

    public OfType(TIterator source)
    {
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
        while (_source.TryGetNext(out var value)) {
            if (value is TResult v) {
                current = v;
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
