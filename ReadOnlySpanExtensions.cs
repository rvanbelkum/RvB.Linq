using RvB.Linq;
using System.Diagnostics.CodeAnalysis;

namespace RvB.Linq;

public static partial class ReadOnlySpanExtensions
{
#if NET10_0_OR_GREATER
    extension<TSource>(ReadOnlySpan<char> source)
    {
        public Iterable<Select<ReadOnlySpanIterator<char>, char, TResult>, TResult> Select<TResult>(Func<char, TResult> selector)
            => new(new(new(source), selector));

        public ReadOnlySpan<char> Trim(out bool trimmedBegin, out bool trimmedEnd)
        {
            if (source.Length == 0) {
                trimmedBegin = trimmedEnd = false;
                return source;
            }
            trimmedBegin = char.IsWhiteSpace(source[0]);
            trimmedEnd = char.IsWhiteSpace(source[^1]);
            if (!trimmedBegin && !trimmedEnd) {
                return source;
            }
            return source.Trim();
        }
    }

    extension<T>(ReadOnlySpan<T> source)
    {
        public T? First(Func<T, bool> predicate)
        {
            if (source.TryGetFirst(predicate, out var first)) {
                return first;
            }
            throw new Exception("Not found");
        }

        public T? Last(Func<T, bool> predicate)
        {
            if (source.TryGetLast(predicate, out var first)) {
                return first;
            }
            throw new Exception("Not found");
        }

        public bool TryGetFirst(Func<T, bool> predicate, [MaybeNullWhen(false)] out T first)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            for (int i = 0; i < source.Length; i += 1) {
                if (predicate(source[i])) {
                    first = source[i];
                    return true;
                }
            }
            first = default;
            return false;
        }

        public bool TryGetLast(Func<T, bool> predicate, [MaybeNullWhen(false)] out T last)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            for (int i = source.Length - 1; i >= 0; i -= 1) {
                if (predicate(source[i])) {
                    last = source[i];
                    return true;
                }
            }
            last = default;
            return false;
        }

        public void Deconstruct(out T item1, out T item2)
        {
            item1 = source[0];
            item2 = source[1];
        }

        public void Deconstruct(out T item1, out T item2, out T item3)
        {
            item1 = source[0];
            item2 = source[1];
            item3 = source[2];
        }

        public void Deconstruct(out T item1, out T item2, out T item3, out T item4)
        {
            item1 = source[0];
            item2 = source[1];
            item3 = source[2];
            item4 = source[3];
        }

        public ReadOnlySpan<T> Intersect(ReadOnlySpan<T> second, IEqualityComparer<T>? comparer = null)
        {
            var set = new HashSet<T>(second.ToArray(), comparer);
            var intersection = new HashSet<T>(comparer);
            foreach (var element in source) {
                if (set.Remove(element)) {
                    intersection.Add(element);
                }
            }
            return intersection.ToArray();
        }

        public int Count(Func<T, bool> selector)
        {
            var count = 0;
            foreach (var item in source) {
                if (selector(item)) {
                    count += 1;
                }
            }
            return count;
        }

        public T? Min(IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            if (comparer == Comparer<T>.Default) {
                if (typeof(T) == typeof(byte)) {
                    return SimdExtensions.Min<T, byte>(source);
                } else if (typeof(T) == typeof(sbyte)) {
                    return SimdExtensions.Min<T, sbyte>(source);
                } else if (typeof(T) == typeof(short)) {
                    return SimdExtensions.Min<T, short>(source);
                } else if (typeof(T) == typeof(ushort)) {
                    return SimdExtensions.Min<T, ushort>(source);
                } else if (typeof(T) == typeof(int)) {
                    return SimdExtensions.Min<T, int>(source);
                } else if (typeof(T) == typeof(uint)) {
                    return SimdExtensions.Min<T, uint>(source);
                } else if (typeof(T) == typeof(long)) {
                    return SimdExtensions.Min<T, long>(source);
                } else if (typeof(T) == typeof(ulong)) {
                    return SimdExtensions.Min<T, ulong>(source);
                } else if (typeof(T) == typeof(nint)) {
                    return SimdExtensions.Min<T, nint>(source);
                } else if (typeof(T) == typeof(nuint)) {
                    return SimdExtensions.Min<T, nuint>(source);
                } else if (typeof(T) == typeof(Int128)) {
                    return SimdExtensions.Min<T, Int128>(source);
                } else if (typeof(T) == typeof(UInt128)) {
                    return SimdExtensions.Min<T, UInt128>(source);
                }
            }
            return SpanSupport.Min(source, comparer);
        }

        public T? Max(IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            if (comparer == Comparer<T>.Default) {
                if (typeof(T) == typeof(byte)) {
                    return SimdExtensions.Max<T, byte>(source);
                } else if (typeof(T) == typeof(sbyte)) {
                    return SimdExtensions.Max<T, sbyte>(source);
                } else if (typeof(T) == typeof(short)) {
                    return SimdExtensions.Max<T, short>(source);
                } else if (typeof(T) == typeof(ushort)) {
                    return SimdExtensions.Max<T, ushort>(source);
                } else if (typeof(T) == typeof(int)) {
                    return SimdExtensions.Max<T, int>(source);
                } else if (typeof(T) == typeof(uint)) {
                    return SimdExtensions.Max<T, uint>(source);
                } else if (typeof(T) == typeof(long)) {
                    return SimdExtensions.Max<T, long>(source);
                } else if (typeof(T) == typeof(ulong)) {
                    return SimdExtensions.Max<T, ulong>(source);
                } else if (typeof(T) == typeof(nint)) {
                    return SimdExtensions.Max<T, nint>(source);
                } else if (typeof(T) == typeof(nuint)) {
                    return SimdExtensions.Max<T, nuint>(source);
                } else if (typeof(T) == typeof(Int128)) {
                    return SimdExtensions.Max<T, Int128>(source);
                } else if (typeof(T) == typeof(UInt128)) {
                    return SimdExtensions.Max<T, UInt128>(source);
                }
            }
            return SpanSupport.Max(source, comparer);
        }
    }

    extension<T>(Span<T> source)
    {
        public T? Min(IComparer<T>? comparer = null)
        {
            return ((ReadOnlySpan<T>)source).Min(comparer);
        }

        public T? Max(IComparer<T>? comparer = null)
        {
            return ((ReadOnlySpan<T>)source).Max(comparer);
        }
    }

    extension<T>(ReadOnlySpan<T> source) where T : struct, INumber<T>
    {
        public T Sum()
        {
            // Signed
            if (typeof(T) == typeof(int)) {
                return SimdExtensions.SumSignedChecked<T, int>(source);
            } else if (typeof(T) == typeof(long)) {
                return SimdExtensions.SumSignedChecked<T, long>(source);
            } else if (typeof(T) == typeof(sbyte)) {
                return SimdExtensions.SumSignedChecked<T, sbyte>(source);
            } else if (typeof(T) == typeof(short)) {
                return SimdExtensions.SumSignedChecked<T, short>(source);
            }
            // Unsigned
            else if (typeof(T) == typeof(byte)) {
                return SimdExtensions.SumUnsignedChecked<T, byte>(source);
            } else if (typeof(T) == typeof(ushort)) {
                return SimdExtensions.SumUnsignedChecked<T, ushort>(source);
            } else if (typeof(T) == typeof(uint)) {
                return SimdExtensions.SumUnsignedChecked<T, uint>(source);
            } else if (typeof(T) == typeof(ulong)) {
                return SimdExtensions.SumUnsignedChecked<T, ulong>(source);
            }
            // double
            else if (typeof(T) == typeof(double)) {
                // double uses unchecked operation
                return SimdExtensions.SumUnchecked(source);
            } else {
                var sum = T.Zero;
                foreach (var item in source) {
                    checked { sum += T.CreateChecked(item); }
                }
                return sum;
            }
        }

        public double Average()
        {
            if (typeof(T) == typeof(int)) {
                return SimdExtensions.Average(source);
            }
            double sum = 0;
            foreach (var value in source) {
                checked { sum += double.CreateChecked(value); }
            }
            return sum / source.Length;
        }

    }

    extension<T>(Span<T> source) where T : struct, INumber<T>
    {
        public T Sum()
            => ((ReadOnlySpan<T>)source).Sum();

        public double Average()
            => ((ReadOnlySpan<T>)source).Average();
    }

    public static float Average(this ReadOnlySpan<float> span)
    {
        var sum = span.Sum();
        return sum / span.Length;
    }

    public static decimal Average(this ReadOnlySpan<decimal> span)
    {
        var sum = span.Sum();
        return sum / span.Length;
    }

#else
    public static Iterable<Select<ReadOnlySpanIterator<char>, char, TResult>, TResult> Select<TResult>(this ReadOnlySpan<char> source, Func<char, TResult> selector)
       => new(new(new(source), selector));

    public static TSource? First<TSource>(this ReadOnlySpan<TSource> source, Func<TSource, bool> predicate)
    {
        if (source.TryGetFirst(predicate, out var first))
            return first;
        throw new Exception("Not found");
    }

    public static TSource? Last<TSource>(this ReadOnlySpan<TSource> source, Func<TSource, bool> predicate)
    {
        if (source.TryGetLast(predicate, out var first))
            return first;
        throw new Exception("Not found");
    }

    public static bool TryGetFirst<TSource>(this ReadOnlySpan<TSource> source, Func<TSource, bool> predicate, [MaybeNullWhen(false)] out TSource first)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        for (int i = 0; i < source.Length; i += 1) {
            if (predicate(source[i])) {
                first = source[i];
                return true;
            }
        }
        first = default;
        return false;
    }

    public static bool TryGetLast<TSource>(this ReadOnlySpan<TSource> source, Func<TSource, bool> predicate, [MaybeNullWhen(false)] out TSource last)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        for (int i = source.Length - 1; i >= 0; i -= 1) {
            if (predicate(source[i])) {
                last = source[i];
                return true;
            }
        }
        last = default;
        return false;
    }

    public static void Deconstruct<T>(this ReadOnlySpan<T> span, out T item1, out T item2)
    {
        item1 = span[0];
        item2 = span[1];
    }

    public static void Deconstruct<T>(this ReadOnlySpan<T> span, out T item1, out T item2, out T item3)
    {
        item1 = span[0];
        item2 = span[1];
        item3 = span[2];
    }

    public static void Deconstruct<T>(this ReadOnlySpan<T> span, out T item1, out T item2, out T item3, out T item4)
    {
        item1 = span[0];
        item2 = span[1];
        item3 = span[2];
        item4 = span[3];
    }

    public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, out bool trimmedBegin, out bool trimmedEnd)
    {
        if (span.Length == 0) {
            trimmedBegin = trimmedEnd = false;
            return span;
        }
        trimmedBegin = char.IsWhiteSpace(span[0]);
        trimmedEnd = char.IsWhiteSpace(span[^1]);
        if (!trimmedBegin && !trimmedEnd) {
            return span;
        }
        return span.Trim();
    }

    public static ReadOnlySpan<TSource> Intersect<TSource>(this ReadOnlySpan<TSource> first, ReadOnlySpan<TSource> second, IEqualityComparer<TSource>? comparer = null)
    {
        var set = new HashSet<TSource>(second.ToArray(), comparer);
        var intersection = new HashSet<TSource>(comparer);
        foreach (var element in first) {
            if (set.Remove(element)) {
                intersection.Add(element);
            }
        }
        return intersection.ToArray();
    }

    public static int Count<TSource>(this ReadOnlySpan<TSource> source, Func<TSource, bool> selector)
    {
        var count = 0;
        foreach (var item in source) {
            if (selector(item))
                count += 1;
        }
        return count;
    }

    public static T? Min<T>(this Span<T> span, IComparer<T>? comparer = null)
    {
        return Min((ReadOnlySpan<T>)span, comparer);
    }

    public static T? Min<T>(this ReadOnlySpan<T> span, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        if (comparer == Comparer<T>.Default) {
            if (typeof(T) == typeof(byte)) {
                return SimdExtensions.Min<T, byte>(span);
            } else if (typeof(T) == typeof(sbyte)) {
                return SimdExtensions.Min<T, sbyte>(span);
            } else if (typeof(T) == typeof(short)) {
                return SimdExtensions.Min<T, short>(span);
            } else if (typeof(T) == typeof(ushort)) {
                return SimdExtensions.Min<T, ushort>(span);
            } else if (typeof(T) == typeof(int)) {
                return SimdExtensions.Min<T, int>(span);
            } else if (typeof(T) == typeof(uint)) {
                return SimdExtensions.Min<T, uint>(span);
            } else if (typeof(T) == typeof(long)) {
                return SimdExtensions.Min<T, long>(span);
            } else if (typeof(T) == typeof(ulong)) {
                return SimdExtensions.Min<T, ulong>(span);
            } else if (typeof(T) == typeof(nint)) {
                return SimdExtensions.Min<T, nint>(span);
            } else if (typeof(T) == typeof(nuint)) {
                return SimdExtensions.Min<T, nuint>(span);
            } else if (typeof(T) == typeof(Int128)) {
                return SimdExtensions.Min<T, Int128>(span);
            } else if (typeof(T) == typeof(UInt128)) {
                return SimdExtensions.Min<T, UInt128>(span);
            }
        }
        return SpanSupport.Min(span, comparer);
    }

    public static T? Max<T>(this Span<T> span, IComparer<T>? comparer = null)
    {
        return Max((ReadOnlySpan<T>)span, comparer);
    }

    public static T? Max<T>(this ReadOnlySpan<T> span, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        if (comparer == Comparer<T>.Default) {
            if (typeof(T) == typeof(byte)) {
                return SimdExtensions.Max<T, byte>(span);
            } else if (typeof(T) == typeof(sbyte)) {
                return SimdExtensions.Max<T, sbyte>(span);
            } else if (typeof(T) == typeof(short)) {
                return SimdExtensions.Max<T, short>(span);
            } else if (typeof(T) == typeof(ushort)) {
                return SimdExtensions.Max<T, ushort>(span);
            } else if (typeof(T) == typeof(int)) {
                return SimdExtensions.Max<T, int>(span);
            } else if (typeof(T) == typeof(uint)) {
                return SimdExtensions.Max<T, uint>(span);
            } else if (typeof(T) == typeof(long)) {
                return SimdExtensions.Max<T, long>(span);
            } else if (typeof(T) == typeof(ulong)) {
                return SimdExtensions.Max<T, ulong>(span);
            } else if (typeof(T) == typeof(nint)) {
                return SimdExtensions.Max<T, nint>(span);
            } else if (typeof(T) == typeof(nuint)) {
                return SimdExtensions.Max<T, nuint>(span);
            } else if (typeof(T) == typeof(Int128)) {
                return SimdExtensions.Max<T, Int128>(span);
            } else if (typeof(T) == typeof(UInt128)) {
                return SimdExtensions.Max<T, UInt128>(span);
            }
        }
        return SpanSupport.Max(span, comparer);
    }

    public static T Sum<T>(this Span<T> span)
        where T : struct, INumber<T>
    {
        return Sum((ReadOnlySpan<T>)span);
    }

    public static T Sum<T>(this ReadOnlySpan<T> span)
        where T : struct, INumber<T>
    {
        // Signed
        if (typeof(T) == typeof(int)) {
            return SimdExtensions.SumSignedChecked<T, int>(span);
        } else if (typeof(T) == typeof(long)) {
            return SimdExtensions.SumSignedChecked<T, long>(span);
        } else if (typeof(T) == typeof(sbyte)) {
            return SimdExtensions.SumSignedChecked<T, sbyte>(span);
        } else if (typeof(T) == typeof(short)) {
            return SimdExtensions.SumSignedChecked<T, short>(span);
        }
        // Unsigned
        else if (typeof(T) == typeof(byte)) {
            return SimdExtensions.SumUnsignedChecked<T, byte>(span);
        } else if (typeof(T) == typeof(ushort)) {
            return SimdExtensions.SumUnsignedChecked<T, ushort>(span);
        } else if (typeof(T) == typeof(uint)) {
            return SimdExtensions.SumUnsignedChecked<T, uint>(span);
        } else if (typeof(T) == typeof(ulong)) {
            return SimdExtensions.SumUnsignedChecked<T, ulong>(span);
        }
        // double
        else if (typeof(T) == typeof(double)) {
            // double uses unchecked operation
            return SimdExtensions.SumUnchecked(span);
        } else {
            var sum = T.Zero;
            foreach (var item in span) {
                checked { sum += T.CreateChecked(item); }
            }
            return sum;
        }
    }

    public static float Average(this ReadOnlySpan<float> span)
    {
        var sum = span.Sum();
        return sum / span.Length;
    }

    public static decimal Average(this ReadOnlySpan<decimal> span)
    {
        var sum = span.Sum();
        return sum / span.Length;
    }

    public static double Average<T>(this ReadOnlySpan<T> span)
        where T : struct, INumber<T>
    {
        if (typeof(T) == typeof(int)) {
            return SimdExtensions.Average(span);
        }
        double sum = 0;
        foreach (var value in span) {
            checked { sum += double.CreateChecked(value); }
        }
        return sum / span.Length;
    }
#endif
}
