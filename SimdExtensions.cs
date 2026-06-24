using RvB.Linq.Utils;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RvB.Linq;

internal static class SimdExtensions
{
    public static T Min<T, U>(ReadOnlySpan<T> span)
        where U : struct, IBinaryInteger<U>
    {
        var uSpan = Unsafe.BitCast<ReadOnlySpan<T>, ReadOnlySpan<U>>(span);
        var result = Min(uSpan);
        return Unsafe.As<U, T>(ref result);
    }

    public static T Max<T, U>(ReadOnlySpan<T> span)
        where U : struct, IBinaryInteger<U>
    {
        var uSpan = Unsafe.BitCast<ReadOnlySpan<T>, ReadOnlySpan<U>>(span);
        var result = Max(uSpan);
        return Unsafe.As<U, T>(ref result);
    }

    public static T Min<T>(ReadOnlySpan<T> span)
        where T : struct, IBinaryInteger<T>
    {
        if (span.Length == 0) {
            Throws.NoElements();
        }
        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && span.Length >= Vector<T>.Count) {
            ref var current = ref MemoryMarshal.GetReference(span);
            ref var end = ref Unsafe.Add(ref current, span.Length);
            ref var to = ref Unsafe.Subtract(ref end, Vector<T>.Count);

            var vectorResult = Vector.LoadUnsafe(ref current);
            current = ref Unsafe.Add(ref current, Vector<T>.Count);
            while (Unsafe.IsAddressLessThan(ref current, ref to)) // exclude last
            {
                var data = Vector.LoadUnsafe(ref current);
                vectorResult = Vector.Min(data, vectorResult);
                current = ref Unsafe.Add(ref current, Vector<T>.Count);
            }

            // overlap load
            var lastOverlap = Vector.LoadUnsafe(ref to);
            vectorResult = Vector.Min(lastOverlap, vectorResult);

            var result = vectorResult[0];
            for (int i = 1; i < Vector<T>.Count; i++) {
                if (vectorResult[i] < result) {
                    result = vectorResult[i];
                }
            }
            return result;
        } else {
            var result = span[0];
            for (int i = 1; i < span.Length; i++) {
                if (span[i] < result) {
                    result = span[i];
                }
            }
            return result;
        }
    }

    public static T Max<T>(ReadOnlySpan<T> span)
        where T : struct, IBinaryInteger<T>
    {
        if (span.Length == 0) {
            Throws.NoElements();
        }
        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && span.Length >= Vector<T>.Count) {
            ref var current = ref MemoryMarshal.GetReference(span);
            ref var end = ref Unsafe.Add(ref current, span.Length);
            ref var to = ref Unsafe.Subtract(ref end, Vector<T>.Count);

            var vectorResult = Vector.LoadUnsafe(ref current);
            current = ref Unsafe.Add(ref current, Vector<T>.Count);
            while (Unsafe.IsAddressLessThan(ref current, ref to)) // exclude last
            {
                var data = Vector.LoadUnsafe(ref current);
                vectorResult = Vector.Max(data, vectorResult);
                current = ref Unsafe.Add(ref current, Vector<T>.Count);
            }

            // overlap load
            var lastOverlap = Vector.LoadUnsafe(ref to);
            vectorResult = Vector.Max(lastOverlap, vectorResult);

            var result = vectorResult[0];
            for (int i = 1; i < Vector<T>.Count; i++) {
                if (vectorResult[i] > result) {
                    result = vectorResult[i];
                }
            }
            return result;
        } else {
            var result = span[0];
            for (int i = 1; i < span.Length; i++) {
                if (span[i] > result) {
                    result = span[i];
                }
            }
            return result;
        }
    }

    public static T SumSignedChecked<T, U>(ReadOnlySpan<T> span)
        where U : struct, ISignedNumber<U>, IMinMaxValue<U>, IBinaryInteger<U>
    {
        var uSpan = Unsafe.BitCast<ReadOnlySpan<T>, ReadOnlySpan<U>>(span);
        var result = SumSignedChecked(uSpan);
        return Unsafe.As<U, T>(ref result);
    }

    public static T SumUnsignedChecked<T, U>(ReadOnlySpan<T> span)
        where U : struct, IUnsignedNumber<U>, IMinMaxValue<U>, IBinaryInteger<U>
    {
        var uSpan = Unsafe.BitCast<ReadOnlySpan<T>, ReadOnlySpan<U>>(span);
        var result = SumUnsignedChecked(uSpan);
        return Unsafe.As<U, T>(ref result);
    }

    public static T SumSignedChecked<T>(ReadOnlySpan<T> span)
       where T : struct, ISignedNumber<T>, IMinMaxValue<T>, IBinaryInteger<T>
    {
        ref var current = ref MemoryMarshal.GetReference(span);
        ref var end = ref Unsafe.Add(ref current, span.Length);

        var result = T.Zero;

        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && span.Length >= Vector<T>.Count) {
            ref var to = ref Unsafe.Subtract(ref end, Vector<T>.Count);

            var vectorSum = Vector<T>.Zero;
            var overflowTest = new Vector<T>(T.MinValue);
            do {
                var data = Vector.LoadUnsafe(ref current);
                var sum = vectorSum + data;

                // Check for overflow for each value.
                // This technique uses the same checks as described in Hacker's Delight.
                // sum = a + b;
                // overflow = (sum ^ a) & (sum ^ b);
                var overflow = (sum ^ vectorSum) & (sum ^ data);
                if ((overflow & overflowTest) != Vector<T>.Zero) {
                    throw new OverflowException();
                }

                vectorSum = sum;
                current = ref Unsafe.Add(ref current, Vector<T>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref current, ref to)); // (current <= to) -> !(current > to) 

            // Perform the final addition using checked. 
            // Do not use Vector.Sum as it does not check for overflow.
            for (int i = 0; i < Vector<T>.Count; i++) {
                checked { result += vectorSum[i]; }
            }
        }
        // fill rest
        while (Unsafe.IsAddressLessThan(ref current, ref end)) {
            checked { result += current; }
            current = ref Unsafe.Add(ref current, 1);
        }
        return result;
    }

    public static T SumUnsignedChecked<T>(ReadOnlySpan<T> span)
        where T : struct, IUnsignedNumber<T>, IMinMaxValue<T>, IBinaryInteger<T>
    {
        ref var current = ref MemoryMarshal.GetReference(span);
        ref var end = ref Unsafe.Add(ref current, span.Length);

        var result = T.Zero;

        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && span.Length >= Vector<T>.Count) {
            ref var to = ref Unsafe.Subtract(ref end, Vector<T>.Count);
            var vectorSum = Vector<T>.Zero;
            do {
                var data = Vector.LoadUnsafe(ref current);
                var sum = vectorSum + data;

                // overflow check: sum < vectorSum
                var overflow = Vector.LessThan(sum, vectorSum);
                if (Vector.GreaterThanAny(overflow, Vector<T>.Zero)) {
                    throw new OverflowException();
                }
                vectorSum = sum;
                current = ref Unsafe.Add(ref current, Vector<T>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref current, ref to));

            // Perform the final addition using checked. 
            // Do not use Vector.Sum as it does not check for overflow.
            for (int i = 0; i < Vector<T>.Count; i++) {
                checked { result += vectorSum[i]; }
            }
        }
        // fill rest
        while (Unsafe.IsAddressLessThan(ref current, ref end)) {
            checked { result += current; }
            current = ref Unsafe.Add(ref current, 1);
        }
        return result;
    }

    public static T SumUnchecked<T>(ReadOnlySpan<T> span)
        where T : struct, INumberBase<T>
    {
        ref var current = ref MemoryMarshal.GetReference(span);
        ref var end = ref Unsafe.Add(ref current, span.Length);

        var result = T.Zero;

        if (Vector.IsHardwareAccelerated && Vector<T>.IsSupported && span.Length >= Vector<T>.Count) {
            ref var to = ref Unsafe.Subtract(ref end, Vector<T>.Count);
            var vectorSum = Vector<T>.Zero;
            do {
                var data = Vector.LoadUnsafe(ref current);
                vectorSum += data;
                current = ref Unsafe.Add(ref current, Vector<T>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref current, ref to)); // (current <= to) -> !(current > to) 

            result = Vector.Sum(vectorSum);
        }

        // fill rest
        while (Unsafe.IsAddressLessThan(ref current, ref end)) {
            unchecked { result += current; } // use unchecked
            current = ref Unsafe.Add(ref current, 1);
        }

        return result;
    }

    public static double Average<T>(ReadOnlySpan<T> span)
    {
        var uSpan = Unsafe.BitCast<ReadOnlySpan<T>, ReadOnlySpan<int>>(span);
        return Average(uSpan);
    }

    public static double Average(ReadOnlySpan<int> span)
    {
        // based on SimdSumNumberUnchecked<T>
        // int[int.MaxValue] { int.MaxValue... }.Sum() is lower than long.MaxValue
        // so Sum as long without overflow check is safe.

        ref var current = ref MemoryMarshal.GetReference(span);
        ref var end = ref Unsafe.Add(ref current, span.Length);
        var sum = 0L;

        if (Vector.IsHardwareAccelerated && Vector<int>.IsSupported && span.Length >= Vector<int>.Count) {
            ref var to = ref Unsafe.Subtract(ref end, Vector<int>.Count);
            var vectorSum = Vector<long>.Zero; // <0, 0, 0, 0> : Vector<long>
            do {
                var data = Vector.LoadUnsafe(ref current);     // <1, 2, 3, 4, 5, 6, 7, 8>   : Vector<int>
                Vector.Widen(data, out var low, out var high); // <1, 2, 3, 4>, <5, 6, 7, 8> : Vector<long>
                vectorSum += low;  // add low  <1, 2, 3, 4>
                vectorSum += high; // and high <6, 8, 10, 12>
                current = ref Unsafe.Add(ref current, Vector<int>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref current, ref to)); // (current <= to) -> !(current > to) 

            sum = Vector.Sum(vectorSum);
        }

        // fill rest
        while (Unsafe.IsAddressLessThan(ref current, ref end)) {
            unchecked { sum += current; } // use unchecked
            current = ref Unsafe.Add(ref current, 1);
        }

        return (double)sum / span.Length;
    }

    /// <summary>Fills the <paramref name="destination"/> with incrementing numbers, starting from <paramref name="value"/>.</summary>
    /// <remarks>Borrowed from .NET source: https://source.dot.net/#System.Linq/System/Linq/Range.cs</remarks>
    public static void FillIncrementing(Span<int> destination, int value)
    {
        ref int pos = ref MemoryMarshal.GetReference(destination);
        ref int end = ref Unsafe.Add(ref pos, destination.Length);

        if (Vector.IsHardwareAccelerated &&
            destination.Length >= Vector<int>.Count) {
            Vector<int> init = Vector<int>.Indices;
            Vector<int> current = new Vector<int>(value) + init;
            Vector<int> increment = new Vector<int>(Vector<int>.Count);

            ref int oneVectorFromEnd = ref Unsafe.Subtract(ref end, Vector<int>.Count);
            do {
                current.StoreUnsafe(ref pos);
                current += increment;
                pos = ref Unsafe.Add(ref pos, Vector<int>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref pos, ref oneVectorFromEnd));

            value = current[0];
        }
        while (Unsafe.IsAddressLessThan(ref pos, ref end)) {
            pos = value++;
            pos = ref Unsafe.Add(ref pos, 1);
        }
    }
}
