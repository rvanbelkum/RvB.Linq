using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
}

public ref struct Cast<TIterator, TSource, TResult> : IIterator<TResult>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier

    public Cast(TIterator source)
    {
        _source = source;
    }

    public readonly bool TryGetCount(out int count)
    {
        if (_source.TryGetCount(out count)) {
            return true;
        }
        return false;
    }

    public bool TryGet(Index index, out TResult item)
    {
        if (_source.TryGet(index, out var value)) {
            item = (TResult)(object)value!;
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TResult current)
    {
        while (_source.TryGetNext(out var value)) {
            var v = (TResult)(object)value!;
            current = v;
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }
}
