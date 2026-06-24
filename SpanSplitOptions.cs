namespace RvB.Linq;

//
// Summary:
//     Specifies options for applicable span Splitting method overloads (e.g. SplitSpan and SplitSpanAny).
[Flags]
public enum SpanSplitOptions
{
    //
    // Summary:
    //     Use the default options when splitting spans.
    None = 0,
    //
    // Summary:
    //     Omit elements that contain an empty string from the result.
    //
    //     If SpanSplitOptions.RemoveEmptyEntries and SpanSplitOptions.TrimEntries
    //     are specified together, then substrings that consist only of white-space characters
    //     are also removed from the result.
    RemoveEmptyEntries = 1,
    //
    // Summary:
    //     Trim white-space characters from each element in the result.
    //
    //     If SpanSplitOptions.RemoveEmptyEntries and SpanSplitOptions.TrimEntries
    //     are specified together, then substrings that consist only of white-space characters
    //     are also removed from the result.
    TrimEntries = 2,

    //
    // Summary: 
    //     Enumerate the elements in reversed order.
    Reverse = 64
}
