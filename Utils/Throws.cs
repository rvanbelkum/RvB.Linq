using System.Diagnostics.CodeAnalysis;

namespace RvB.Linq.Utils;

internal static class Throws
{
    [DoesNotReturn]
    public static void NoElements() => throw new InvalidOperationException("Sequence contains no elements");

    [DoesNotReturn]
    public static void MoreThanOneElement() => throw new InvalidOperationException("Sequence contains more than one element"); // for Single

    [DoesNotReturn]
    public static void MoreThanOneMatch() => throw new InvalidOperationException("Sequence contains more than one matching element"); // for single with predicate

    [DoesNotReturn]
    public static T NoElements<T>() where T : allows ref struct => throw new InvalidOperationException("Sequence contains no elements");

    [DoesNotReturn]
    public static T NoMatch<T>() where T : allows ref struct => throw new InvalidOperationException("Sequence contains no matching element"); // for first, last, single with predicate
}
