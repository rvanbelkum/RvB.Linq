using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
}

public ref struct OfNumber<TIterator, TSource, TNumber> : IIterator<TNumber>
    where TIterator : struct, IIterator<TSource>, allows ref struct
    where TNumber : INumber<TNumber>
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _iterator;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly IFormatProvider? _provider;

    public OfNumber(TIterator iterator, IFormatProvider? provider)
    {
        _iterator = iterator;
        _provider = provider;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TNumber item)
    {
        item = default!;
        return false;
    }

    public bool TryGetNext(out TNumber current)
    {
        while (_iterator.TryGetNext(out var item)) {
            if (item is string str && TNumber.TryParse(str, _provider, out current!)) {
                return true;
            } else if (item is StringRange range && TNumber.TryParse(range.AsSpan(), _provider, out current!)) {
                return true;
            } else if (item != null && TNumber.TryParse(item.ToString(), _provider, out current!)) {
                return true;
            }
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose()
    {
        _iterator.Dispose();
    }
}
