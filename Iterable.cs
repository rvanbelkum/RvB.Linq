using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public ref struct Iterable<TIterator, TSource> : IDisposable
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
    internal TIterator _iterator;

    public TIterator Enumerator => _iterator;

    public Iterable(TIterator iterator)
    {
        _iterator = iterator;
    }

    public readonly TSource this[int index]
        => IterableExtensions.ElementAt(this, index);

    public readonly TSource this[Index index]
        => IterableExtensions.ElementAt(this, index);

    public readonly Iterable<OfType<TIterator, TSource, TResult>, TResult> OfType<TResult>()
    {
        return new(new(_iterator));
    }

    public readonly Iterable<OfNumber<TIterator, TSource, TNumber>, TNumber> OfNumber<TNumber>(IFormatProvider? provider = null)
        where TNumber : INumber<TNumber>
    {
        return new(new(_iterator, provider));
    }

    public readonly Iterable<Cast<TIterator, TSource, TResult>, TResult> Cast<TResult>()
    {
        return new(new(_iterator));
    }

    public bool TryTake(out TSource item)
        => TryTake(out item, true);

    public bool TryTake(out TSource item1, out TSource item2)
        => TryTake(out item1, out item2, true);

    public bool TryTake(out TSource item1, out TSource item2, out TSource item3)
        => TryTake(out item1, out item2, out item3, true);

    public bool TryTake(out TSource item1, out TSource item2, out TSource item3, out TSource item4)
        => TryTake(out item1, out item2, out item3, out item4, true);

    public bool TryTake<T>([MaybeNullWhen(false)] out T number1, IFormatProvider? provider = null)
        where T : INumber<T>
    {
        if (TryTakeAndParse(out number1, provider)) {
            return true;
        }
        number1 = default;
        return false;
    }

    public bool TryTake<T>([MaybeNullWhen(false)] out T number1, [MaybeNullWhen(false)] out T number2, IFormatProvider? provider = null)
        where T : INumber<T>
    {
        if (TryTakeAndParse(out number1, provider) && TryTakeAndParse(out number2, provider)) {
            return true;
        }
        number1 = number2 = default;
        return false;
    }

    public bool TryTake<T>([MaybeNullWhen(false)] out T number1, [MaybeNullWhen(false)] out T number2, [MaybeNullWhen(false)] out T number3, IFormatProvider? provider = null)
        where T : INumber<T>
    {
        if (TryTakeAndParse(out number1, provider) && TryTakeAndParse(out number2, provider) && TryTakeAndParse(out number3, provider)) {
            return true;
        }
        number1 = number2 = number3 = default;
        return false;
    }

    public bool TryTake<T>([MaybeNullWhen(false)] out T number1, [MaybeNullWhen(false)] out T number2, [MaybeNullWhen(false)] out T number3, [MaybeNullWhen(false)] out T number4, IFormatProvider? provider = null)
        where T : INumber<T>
    {
        if (TryTakeAndParse(out number1, provider) && TryTakeAndParse(out number2, provider) && TryTakeAndParse(out number3, provider) && TryTakeAndParse(out number4, provider)) {
            return true;
        }
        number1 = number2 = number3 = number4 = default;
        return false;
    }

    private bool TryTakeAndParse<T>([MaybeNullWhen(false)] out T number, IFormatProvider? provider)
        where T : INumber<T>
    {
        if (TryTake(out var item)) {
            if (item is string str) {
                return T.TryParse(str, provider, out number);
            }
            if (item is StringRange range) {
                return T.TryParse(range.AsSpan(), provider, out number);
            }
            if (item != null) {
                return T.TryParse(item.ToString(), provider, out number);
            }
        }
        Unsafe.SkipInit(out number);
        return false;
    }

    public void Deconstruct(out TSource item1, out TSource item2)
        => _ = TryTake(out item1, out item2, false);

    public void Deconstruct(out TSource item1, out TSource item2, out TSource item3)
        => _ = TryTake(out item1, out item2, out item3, false);

    public void Deconstruct(out TSource item1, out TSource item2, out TSource item3, out TSource item4)
        => _ = TryTake(out item1, out item2, out item3, out item4, false);

    private TSource _current = default!;

    public readonly TSource Current {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _current;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        if (_iterator.TryGetNext(out _current)) {
            return true;
        } else {
            _current = default!;
            return false;
        }
    }

    public readonly Iterable<TIterator, TSource> GetEnumerator() => this;

    internal readonly bool TryGet(int index, out TSource element)
    {
        if (_iterator.TryGet(index, out element)) {
            return true;
        }
        using (var iterator = _iterator) {
            while (index > 0 && iterator.TryGetNext(out _)) {
                index -= 1;
            }
            if (index == 0 && iterator.TryGetNext(out var value)) {
                element = value;
                return true;
            }
            element = default!;
            return false;
        }
    }

    internal readonly bool TryGet(Index index, out TSource element)
    {
        if (_iterator.TryGet(index, out element)) {
            return true;
        }
        using (var iterator = _iterator) {
            var idx = index.Value;
            if (index.IsFromEnd) {
                var buffer = this.ToArray();// IteratorExtensions.ToArray<TIterator, TSource>(iterator);
                idx = buffer.Length - idx;
                if (idx >= 0) {
                    element = buffer[idx];
                    return true;
                }
            } else {
                while (idx > 0 && iterator.TryGetNext(out _)) {
                    idx -= 1;
                }
                if (idx == 0 && iterator.TryGetNext(out var value)) {
                    element = value;
                    return true;
                }
            }
            element = default!;
            return false;
        }
    }

    private bool TryTake(out TSource item, bool saveProgress)
    {
        var iterator = _iterator;
        if (!iterator.TryGetNext(out var value)) {
            goto failed;
        }
        item = value;
        if (saveProgress) {
            _iterator.Dispose();
            _iterator = iterator;
        }
        iterator.Dispose();
        return true;
    failed:
        iterator.Dispose();
        Unsafe.SkipInit(out item);
        return false;
    }

    private bool TryTake(out TSource item1, out TSource item2, bool saveProgress)
    {
        var iterator = _iterator;
        if (!iterator.TryGetNext(out var value)) {
            goto failed;
        }
        item1 = value;
        if (!iterator.TryGetNext(out value)) {
            goto failed;
        }
        item2 = value;
        if (saveProgress) {
            _iterator.Dispose();
            _iterator = iterator;
        }
        iterator.Dispose();
        return true;
    failed:
        iterator.Dispose();
        Unsafe.SkipInit(out item1);
        Unsafe.SkipInit(out item2);
        return false;
    }

    private bool TryTake(out TSource item1, out TSource item2, out TSource item3, bool saveProgress)
    {
        var iterator = _iterator;
        if (!iterator.TryGetNext(out var value)) {
            goto failed;
        }
        item1 = value;
        if (!iterator.TryGetNext(out value)) {
            goto failed;
        }
        item2 = value;
        if (!iterator.TryGetNext(out value)) {
            goto failed;
        }
        item3 = value;
        if (saveProgress) {
            _iterator.Dispose();
            _iterator = iterator;
        }
        iterator.Dispose();
        return true;
    failed:
        iterator.Dispose();
        Unsafe.SkipInit(out item1);
        Unsafe.SkipInit(out item2);
        Unsafe.SkipInit(out item3);
        return false;
    }

    private bool TryTake(out TSource item1, out TSource item2, out TSource item3, out TSource item4, bool saveProgress)
    {
        var iterator = _iterator;
        if (!iterator.TryGetNext(out var value)) {
            goto failed;
        }
        item1 = value;
        if (!iterator.TryGetNext(out value)) {
            goto failed;
        }
        item2 = value;
        if (!iterator.TryGetNext(out value)) {
            goto failed;
        }
        item3 = value;
        if (!iterator.TryGetNext(out value)) {
            goto failed;
        }
        item4 = value;
        if (saveProgress) {
            _iterator.Dispose();
            _iterator = iterator;
        }
        iterator.Dispose();
        return true;
    failed:
        iterator.Dispose();
        Unsafe.SkipInit(out item1);
        Unsafe.SkipInit(out item2);
        Unsafe.SkipInit(out item3);
        Unsafe.SkipInit(out item4);
        return false;
    }

    public void Dispose()
    {
        _iterator.Dispose();
    }
}
