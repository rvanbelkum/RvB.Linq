using RvB.Linq.Utils;

namespace RvB.Linq;

internal static class SpanSupport
{
    public static TSource? Min<TSource>(ReadOnlySpan<TSource> span, IComparer<TSource> comparer)
    {
        TSource? value = default;
        if (value is null) {
            // reference type
            var index = 0;
            do {
                if (!(index < span.Length)) {
                    return value;
                }
                value = span[index++];
            } while (value is null);

            while (index < span.Length) {
                // compare both(left, right) non-null
                if (span[index] is not null && comparer.Compare(span[index], value) < 0) {
                    value = span[index];
                }
                index++;
            }
            return value;
        } else {
            // value type
            if (span.Length == 0) {
                Throws.NoElements();
            }

            var index = 1;
            value = span[0];

            // optimize for default comparer
            if (comparer == Comparer<TSource>.Default) {
                while (index < span.Length) {
                    if (Comparer<TSource>.Default.Compare(span[index], value) < 0) {
                        value = span[index];
                    }
                    index++;
                }
                return value;
            } else {
                while (index < span.Length) {
                    if (comparer.Compare(span[index], value) < 0) {
                        value = span[index];
                    }
                    index++;
                }
                return value;
            }
        }
    }

    public static TSource? Max<TSource>(ReadOnlySpan<TSource> span, IComparer<TSource> comparer)
    {
        TSource? value = default;
        if (value is null) {
            // reference type
            var index = 0;
            do {
                if (!(index < span.Length)) {
                    return value;
                }
                value = span[index++];
            } while (value is null);

            while (index < span.Length) {
                // compare both(left, right) non-null
                if (span[index] is not null && comparer.Compare(span[index], value) > 0) {
                    value = span[index];
                }
                index++;
            }
            return value;
        } else {
            // value type
            if (span.Length == 0) {
                Throws.NoElements();
            }

            var index = 1;
            value = span[0];

            // optimize for default comparer
            if (comparer == Comparer<TSource>.Default) {
                while (index < span.Length) {
                    if (Comparer<TSource>.Default.Compare(span[index], value) > 0) {
                        value = span[index];
                    }
                    index++;
                }
                return value;
            } else {
                while (index < span.Length) {
                    if (comparer.Compare(span[index], value) > 0) {
                        value = span[index];
                    }
                    index++;
                }
                return value;
            }
        }
    }
}
