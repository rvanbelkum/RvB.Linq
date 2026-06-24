using System.Diagnostics.CodeAnalysis;

namespace RvB.Linq.Utils;

internal class SpanComparer<T> : IComparer<ReadOnlySpan<T>> where T : IComparable<T>
{
    internal static SpanComparer<T> Default { get; } = new();

    // Prevent external creation
    private SpanComparer() { }

    public int Compare(ReadOnlySpan<T> x, ReadOnlySpan<T> y) => x.SequenceCompareTo(y);
}

internal class SpanEqualityComparer<T> : IEqualityComparer<ReadOnlySpan<T>> where T : IEquatable<T>
{
    internal static SpanEqualityComparer<T> Default { get; } = new();

    // Prevent external creation
    private SpanEqualityComparer() { }

    public bool Equals(ReadOnlySpan<T> x, ReadOnlySpan<T> y) => x.SequenceEqual(y);

    public int GetHashCode([DisallowNull] ReadOnlySpan<T> obj) => throw new NotImplementedException();
}
