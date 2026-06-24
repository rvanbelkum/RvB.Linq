using System.Buffers;

namespace RvB.Linq;

public static class AsIteratorExtensions
{
    public static Iterable<ReadOnlySpanIterator<char>, char> AsIterable(this string source)
        => new(new(source));

    public static Iterable<ReadOnlySpanIterator<T>, T> AsIterable<T>(this ReadOnlySpan<T> source)
        => new(new(source));

    public static Iterable<ReadOnlySpanIterator<T>, T> AsIterable<T>(this Span<T> source)
        => new(new(source));

    public static Iterable<ReadOnlySpanIterator<T>, T> AsIterable<T>(this Memory<T> source)
        => new(new(source.Span));

    public static Iterable<ReadOnlySpanIterator<T>, T> AsIterable<T>(this ReadOnlyMemory<T> source)
        => new(new(source.Span));

    public static Iterable<ReadOnlySequenceIterator<T>, T> AsIterable<T>(this ReadOnlySequence<T> source)
        => new(new(source));

    public static Iterable<EnumerableIterator<T>, T> AsIterable<T>(this IEnumerable<T> source)
        => new(new(source));
}
