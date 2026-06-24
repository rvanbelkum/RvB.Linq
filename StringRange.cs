using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace RvB.Linq;

public readonly struct StringRange : IEquatable<StringRange>, IEquatable<string>, IComparable<StringRange>, IComparable<string>, IEnumerable<char>
{
    private readonly string _source;
    private readonly int _start;
    private readonly int _length;

    public StringRange(string source) : this(source, 0, source.Length) { }

    public StringRange(string source, int start) : this(source, start, source.Length - start) { }

    public StringRange(string source, int start, int length)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)start, (uint)source.Length, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)(start + length), (uint)source.Length, nameof(length));
        _source = source;
        _start = start;
        _length = length;
    }

    public StringRange(string source, Range range)
    {
        ArgumentNullException.ThrowIfNull(source);
        var (start, length) = range.GetOffsetAndLength(source.Length);
        _source = source;
        _start = start;
        _length = length;
    }

    public int Length
        => _length;

    public string Source
        => _source;

    public Range Range
        => new(_start, _start + _length);

    public ReadOnlySpan<char> AsSpan()
        => _source.AsSpan(_start, _length);

    public ReadOnlySpan<char> AsSpan(int start)
    {
        if ((uint)start <= (uint)_length) {
            return _source.AsSpan(_start + start, _length - start);
        }
        throw new ArgumentOutOfRangeException(nameof(start));
    }

    public ReadOnlySpan<char> AsSpan(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)start, (uint)_length, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)(start + length), (uint)_length, nameof(length));
        return _source.AsSpan(_start + start, length);
    }

    public char this[int index] {
        get {
            if ((uint)index < (uint)_length) {
                return _source[_start + index];
            }
            throw new IndexOutOfRangeException();
        }
    }

    public char this[Index index] {
        get {
            var idx = index.GetOffset(_length);
            if ((uint)idx < (uint)_length) {
                return _source[_start + idx];
            }
            throw new IndexOutOfRangeException();
        }
    }

    public StringRange this[Range range] {
        get {
            var (start, length) = range.GetOffsetAndLength(_length);
            return new(_source, _start + start, length);
        }
    }

    public StringRange Slice(int start)
    {
        if ((uint)start <= (uint)_length) {
            return new(_source, _start + start, _length - start);
        }
        throw new ArgumentOutOfRangeException(nameof(start));
    }

    public StringRange Slice(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)start, (uint)_length, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)(start + length), (uint)_length, nameof(length));
        return new(_source, _start + start, length);
    }

    public bool StartsWith(ReadOnlySpan<char> text, StringComparison comparisonType = StringComparison.Ordinal)
        => AsSpan().StartsWith(text, comparisonType);

    public bool EndsWith(ReadOnlySpan<char> text, StringComparison comparisonType = StringComparison.Ordinal)
        => AsSpan().EndsWith(text, comparisonType);

    public int IndexOf(char c, int start, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if ((uint)start > (uint)_length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        var index = AsSpan(start).IndexOf(new ReadOnlySpan<char>(ref c), comparisonType);
        if (index != -1) {
            index += start;
        }
        return index;
    }

    public int IndexOf(char c, StringComparison comparisonType = StringComparison.Ordinal)
        => IndexOf(c, 0, comparisonType);

    public int IndexOf(ReadOnlySpan<char> value, int start, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if ((uint)start > (uint)_length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        var index = AsSpan(start).IndexOf(value, comparisonType);
        if (index != -1) {
            index += start;
        }
        return index;
    }

    public int LastIndexOf(ReadOnlySpan<char> value, int start, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if ((uint)start > (uint)_length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        var index = AsSpan(start).LastIndexOf(value, comparisonType);
        if (index != -1) {
            index += start;
        }
        return index;
    }

    public int IndexOf(ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.Ordinal)
        => IndexOf(value, 0, comparisonType);

    public int LastIndexOf(ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.Ordinal)
        => LastIndexOf(value, 0, comparisonType);

    public int IndexOfAny(ReadOnlySpan<char> anyOf, int start)
    {
        if ((uint)start > (uint)_length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        var index = AsSpan(start).IndexOfAny(anyOf);
        if (index != -1) {
            index += start;
        }
        return index;
    }

    public int LastIndexOfAny(ReadOnlySpan<char> anyOf, int start)
    {
        if ((uint)start > (uint)_length) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        var index = AsSpan(start).LastIndexOfAny(anyOf);
        if (index != -1) {
            index += start;
        }
        return index;
    }

    public int IndexOfAny(ReadOnlySpan<char> anyOf)
        => IndexOfAny(anyOf, 0);

    public int LastIndexOfAny(ReadOnlySpan<char> anyOf)
        => LastIndexOfAny(anyOf, 0);

    public StringRange TrimStart()
        => Trim(true, false);

    public StringRange TrimEnd()
        => Trim(false, true);

    public StringRange Trim()
        => Trim(true, true);

    private StringRange Trim(bool left, bool right)
    {
        ReadOnlySpan<char> span = AsSpan();

        int start = 0;
        int end = span.Length;
        if (left) {
            while (start < end && char.IsWhiteSpace(span[start])) {
                start += 1;
            }
        }
        if (right) {
            while (start < end && char.IsWhiteSpace(span[end - 1])) {
                end -= 1;
            }
        }
        return new(_source, _start + start, end - start);
    }

    public StringRange Trim(out bool trimmedLeft, out bool trimmedRight)
    {
        ReadOnlySpan<char> span = AsSpan();
        int start = 0;
        int end = span.Length;
        trimmedLeft = trimmedRight = false;
        while (start < end && char.IsWhiteSpace(span[start])) {
            start += 1;
            trimmedLeft = true;
        }
        while (start < end && char.IsWhiteSpace(span[end - 1])) {
            end -= 1;
            trimmedRight = true;
        }
        return new(_source, _start + start, end - start);
    }

    public StringRange Reverse()
    {
        return new(string.Join("", _source.Substring(_start, _length).Reverse()));
    }

    public override string ToString()
        => AsSpan().ToString();

    public int CompareTo(StringRange other, StringComparison comparisonType)
        => AsSpan().CompareTo(other.AsSpan(), comparisonType);

    public int CompareTo(StringRange other)
        => CompareTo(other, StringComparison.Ordinal);

    public int CompareTo(string? other, StringComparison comparisonType)
        => AsSpan().CompareTo(other, comparisonType);

    public int CompareTo(string? other)
        => CompareTo(other, StringComparison.Ordinal);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => (obj is StringRange range && Equals(range)) || (obj is string str && Equals(str));

    public bool Equals(StringRange other, StringComparison comparisonType)
        => AsSpan().Equals(other.AsSpan(), comparisonType);

    public bool Equals(StringRange other)
        => Equals(other, StringComparison.Ordinal);

    public bool Equals(string? other, StringComparison comparisonType)
        => AsSpan().Equals(other, comparisonType);

    public bool Equals(string? other)
        => Equals(other, StringComparison.Ordinal);

    public bool SequenceEqual(ReadOnlySpan<char> other)
        => AsSpan().SequenceEqual(other);

    public static bool operator ==(StringRange left, StringRange right)
        => left.Equals(right);

    public static bool operator !=(StringRange left, StringRange right)
        => !(left == right);

    public static bool operator ==(string left, StringRange right)
        => right.Equals(left);

    public static bool operator !=(string left, StringRange right)
        => !(left == right);

    public static bool operator ==(StringRange left, string right)
        => left.Equals(right);

    public static bool operator !=(StringRange left, string right)
        => !(left == right);

    public override int GetHashCode()
        => string.GetHashCode(AsSpan(), StringComparison.Ordinal);

    public IEnumerator<char> GetEnumerator()
    {
        for (var i = 0; i < _length; i++) {
            yield return _source[_start + i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator ReadOnlySpan<char>(StringRange stringRange)
        => stringRange.AsSpan();

    public static implicit operator StringRange(string s)
        => new(s);
}

public class StringRangeComparer : IComparer<StringRange>, IEqualityComparer<StringRange>
{
    private readonly StringComparison _comparisonType;

    public static StringRangeComparer InvariantCulture { get; } = new(StringComparison.InvariantCulture);

    public static StringRangeComparer InvariantCultureIgnoreCase { get; } = new(StringComparison.InvariantCultureIgnoreCase);

    public static StringRangeComparer CurrentCulture { get; } = new(StringComparison.CurrentCulture);

    public static StringRangeComparer CurrentCultureIgnoreCase { get; } = new(StringComparison.CurrentCultureIgnoreCase);

    public static StringRangeComparer Ordinal { get; } = new(StringComparison.Ordinal);

    public static StringRangeComparer OrdinalIgnoreCase { get; } = new(StringComparison.OrdinalIgnoreCase);

    private StringRangeComparer(StringComparison comparisonType)
    {
        _comparisonType = comparisonType;
    }

    public int Compare(StringRange x, StringRange y)
    {
        return x.AsSpan().CompareTo(y.AsSpan(), _comparisonType);
    }

    public bool Equals(StringRange x, StringRange y)
    {
        return x.AsSpan().Equals(y.AsSpan(), _comparisonType);
    }

    public int GetHashCode([DisallowNull] StringRange stringRange)
    {
        return string.GetHashCode(stringRange.AsSpan(), _comparisonType);
    }
}
