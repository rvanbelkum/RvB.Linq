namespace RvB.Linq;

public static partial class IterableExtensions
{
    //public static bool TryTake<TIterator, T>(ref this Iterable<TIterator, StringRange> source, [MaybeNullWhen(false)] out T number1, IFormatProvider? provider = null)
    //    where TIterator : struct, IIterator<StringRange>, allows ref struct
    //    where T : INumber<T>
    //{
    //    if (source.TryTake(out var range1)) {
    //        if (T.TryParse(range1, provider, out number1)) {
    //            return true;
    //        }
    //    }
    //    number1 = default;
    //    return false;
    //}

    //public static bool TryTake<TIterator, T>(ref this Iterable<TIterator, StringRange> source, [MaybeNullWhen(false)] out T number1, [MaybeNullWhen(false)] out T number2, IFormatProvider? provider = null)
    //    where TIterator : struct, IIterator<StringRange>, allows ref struct
    //    where T : INumber<T>
    //{
    //    if (source.TryTake(out var range1, out var range2)) {
    //        if (T.TryParse(range1, provider, out number1) && T.TryParse(range2, provider, out number2)) {
    //            return true;
    //        }
    //    }
    //    number1 = number2 = default;
    //    return false;
    //}

    //public static bool TryTake<TIterator, T>(ref this Iterable<TIterator, StringRange> source, [MaybeNullWhen(false)] out T number1, [MaybeNullWhen(false)] out T number2, [MaybeNullWhen(false)] out T number3, IFormatProvider? provider = null)
    //    where TIterator : struct, IIterator<StringRange>, allows ref struct
    //    where T : INumber<T>
    //{
    //    if (source.TryTake(out var range1, out var range2, out var range3)) {
    //        if (T.TryParse(range1, provider, out number1) && T.TryParse(range2, provider, out number2) && T.TryParse(range3, provider, out number3)) {
    //            return true;
    //        }
    //    }
    //    number1 = number2 = number3 = default;
    //    return false;
    //}

    //public static bool TryTake<TIterator, T>(ref this Iterable<TIterator, StringRange> source, [MaybeNullWhen(false)] out T number1, [MaybeNullWhen(false)] out T number2, [MaybeNullWhen(false)] out T number3, [MaybeNullWhen(false)] out T number4, IFormatProvider? provider = null)
    //    where TIterator : struct, IIterator<StringRange>, allows ref struct
    //    where T : INumber<T>
    //{
    //    if (source.TryTake(out var range1, out var range2, out var range3, out var range4)) {
    //        if (T.TryParse(range1, provider, out number1) && T.TryParse(range2, provider, out number2) && T.TryParse(range3, provider, out number3) && T.TryParse(range4, provider, out number4)) {
    //            return true;
    //        }
    //    }
    //    number1 = number2 = number3 = number4 = default;
    //    return false;
    //}
}
