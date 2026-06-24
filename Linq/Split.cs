using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Split, StringRange> SplitSpan(this string source, char separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separator, splitOptions));
    }

    public static Iterable<Split, StringRange> SplitSpan(this string source, string separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separator, splitOptions));
    }

    public static Iterable<Split, StringRange> SplitAnySpan(this string source, ReadOnlySpan<char> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separators, splitOptions));
    }

    public static Iterable<Split, StringRange> SplitAnySpan(this string source, ReadOnlySpan<string> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separators, splitOptions));
    }

    public static Iterable<Split, StringRange> Split(this StringRange source, char separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separator, splitOptions));
    }

    public static Iterable<Split, StringRange> Split(this StringRange source, string separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separator, splitOptions));
    }

    public static Iterable<Split, StringRange> SplitAny(this StringRange source, ReadOnlySpan<char> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separators, splitOptions));
    }

    public static Iterable<Split, StringRange> SplitAny(this StringRange source, ReadOnlySpan<string> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
    {
        return new(new(source, separators, splitOptions));
    }
}

public ref struct Split : IIterator<StringRange>
{
    internal enum SplitMode
    {
        /// <summary>Either a default <see cref="SpanSplitEnumerator"/> was used, or the Iterator has finished enumerating and there's no more work to do.</summary>
        None = 0,
        /// <summary>A single T separator was provided.</summary>
        SingleElement,
        /// <summary>A span of separators was provided, each of which should be treated independently.</summary>
        Any,
        /// <summary>The separator is a span of elements to be treated as a single sequence.</summary>
        Sequence,
        /// <summary>The separator is a span of elements to be treated as a single sequence.</summary>
        MultiSequence,
        /// <summary>The separator is an empty sequence, such that no splits should be performed.</summary>
        EmptySequence,
        /// <summary>
        /// A <see cref="SearchValues{Char}"/> was provided and should behave the same as with <see cref="Any"/> but with the separators in the <see cref="SearchValues"/>
        /// instance instead of in a <see cref="ReadOnlySpan{Char}"/>.
        /// </summary>
        SearchValues
    }

    /// <summary>The input span being split.</summary>
    private readonly StringRange _source;
    private readonly ReadOnlySpan<char> _sourceSpan;

    /// <summary>A single separator to use when <see cref="_splitMode"/> is <see cref="SplitMode.SingleElement"/>.</summary>
    private readonly char _chSeparator = default;
    /// <summary>
    /// A separator span to use when <see cref="_splitMode"/> is <see cref="SplitMode.Sequence"/> (in which case
    /// it's treated as a single separator) or <see cref="SplitMode.Any"/> (in which case it's treated as a set of separators).
    /// </summary>
    private readonly ReadOnlySpan<char> _chSeparators;
    /// <summary>A set of separator sequences to use when <see cref="_splitMode"/> is <see cref="SplitMode.MultiSequence"/>.</summary>
    private readonly ReadOnlySpan<string> _separators;
    /// <summary>A set of separators to use when <see cref="_splitMode"/> is <see cref="SplitMode.SearchValues"/>.</summary>
    private readonly SearchValues<char> _searchValues = default!;
    private SplitMode _splitMode;

    private readonly bool _trimEntries;
    private readonly bool _removeEmptyEntries;
    private readonly bool _reversed;

    /// <summary>The inclusive starting index in <see cref="_source"/> of the current range.</summary>
    private int _startCurrent = 0;
    /// <summary>The exclusive ending index in <see cref="_source"/> of the current range.</summary>
    private int _endCurrent = 0;
    /// <summary>The index in <see cref="_source"/> from which the next separator search should start.</summary>
    private int _startNext;
    private int _endNext;

    private Split(StringRange source, SpanSplitOptions splitOptions)
    {
        _source = source;
        _sourceSpan = source.AsSpan();
        _trimEntries = splitOptions.HasFlag(SpanSplitOptions.TrimEntries);
        _removeEmptyEntries = splitOptions.HasFlag(SpanSplitOptions.RemoveEmptyEntries);
        _reversed = splitOptions.HasFlag(SpanSplitOptions.Reverse);
        if (_reversed) {
            Debugger.Break();
        }
        _startNext = 0;
        _endNext = _sourceSpan.Length;
    }

    public Split(string source, char separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(new(source), splitOptions)
    {
        _chSeparator = separator;
        _splitMode = SplitMode.SingleElement;
    }

    public Split(StringRange source, char separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(source, splitOptions)
    {
        _chSeparator = separator;
        _splitMode = SplitMode.SingleElement;
    }

    public Split(string source, ReadOnlySpan<char> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(new(source), splitOptions)
    {
        _chSeparators = separators;
        _splitMode = SplitMode.Any;
    }

    public Split(StringRange source, ReadOnlySpan<char> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(source, splitOptions)
    {
        _chSeparators = separators;
        _splitMode = SplitMode.Any;
    }

    public Split(string source, string separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(new(source), splitOptions)
    {
        _chSeparators = separator;
        _splitMode = separator.Length == 0 ? SplitMode.EmptySequence : SplitMode.Sequence;
    }

    public Split(StringRange source, string separator, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(source, splitOptions)
    {
        _chSeparators = separator;
        _splitMode = separator.Length == 0 ? SplitMode.EmptySequence : SplitMode.Sequence;
    }

    public Split(string source, ReadOnlySpan<string> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(new(source), splitOptions)
    {
        if (separators.Length == 1) {
            _chSeparators = separators[0];
            _splitMode = SplitMode.Sequence;
        } else {
            _separators = separators;
            _splitMode = SplitMode.MultiSequence;
        }
    }

    public Split(StringRange source, ReadOnlySpan<string> separators, SpanSplitOptions splitOptions = SpanSplitOptions.None)
        : this(source, splitOptions)
    {
        if (separators.Length == 1) {
            _chSeparators = separators[0];
            _splitMode = SplitMode.Sequence;
        } else {
            _separators = separators;
            _splitMode = SplitMode.MultiSequence;
        }
    }

    internal readonly string UltimateSource => _source.Source;

    internal readonly ReadOnlySpan<char> Source => _sourceSpan;

    internal readonly ReadOnlySpan<char> Current {
        get {
            return _sourceSpan[_startCurrent.._endCurrent];
        }
    }
    //internal readonly ReadOnlySpan<char> Remaining => _endNext > _startNext ? _source[_startNext.._endNext] : [];

    public readonly (int Count, int MinLength, int MaxLength) GetCountAndMinMaxLength()
    {
        int count = 0;
        int minLength = int.MaxValue;
        int maxLength = int.MinValue;
        var iterator = GetEnumerator();
        while (iterator.TryGetNext(out var current)) {
            var length = current.Length;
            minLength = int.Min(minLength, length);
            maxLength = int.Max(maxLength, length);
            count += 1;
        }
        return (count, minLength, maxLength);
    }

    public readonly Split GetEnumerator()
        => this;

    public readonly bool TryGetCount(out int count)
    {
        count = default;
        return false;
    }

    public readonly bool TryGet(Index index, out StringRange item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out StringRange current)
    {
        while (MoveForward()) {
            if (_trimEntries) {
                Trim();
            }
            if (!_removeEmptyEntries || _endCurrent > _startCurrent) {
                current = _source[_startCurrent.._endCurrent];
                return true;
            }
        }
        current = default;
        return false;
    }

    public bool TryGetPrev(out StringRange current)
    {
        while (MoveBackward()) {
            if (_trimEntries) {
                Trim();
            }
            if (!_removeEmptyEntries || _endCurrent > _startCurrent) {
                current = _source[_startCurrent.._endCurrent];
                return true;
            }
        }
        current = default;
        return false;
    }

    public readonly void Dispose() { }

    private void Trim()
    {
        while (_startCurrent < _endCurrent && char.IsWhiteSpace(_sourceSpan[_startCurrent])) {
            _startCurrent += 1;
        }
        while (_endCurrent > _startCurrent && char.IsWhiteSpace(_sourceSpan[_endCurrent - 1])) {
            _endCurrent -= 1;
        }
    }

    private bool MoveForward()
    {
        // Search for the next separator index.
        int separatorIndex, separatorLength;
        switch (_splitMode) {
            case SplitMode.None:
                return false;

            case SplitMode.SingleElement:
                separatorIndex = _sourceSpan[_startNext.._endNext].IndexOf(_chSeparator);
                separatorLength = 1;
                break;

            case SplitMode.Any:
                separatorIndex = _sourceSpan[_startNext.._endNext].IndexOfAny(_chSeparators);
                separatorLength = 1;
                break;

            case SplitMode.Sequence:
                separatorIndex = _sourceSpan[_startNext.._endNext].IndexOf(_chSeparators);
                separatorLength = _chSeparators.Length;
                break;

            case SplitMode.MultiSequence:
                separatorIndex = -1;
                separatorLength = 1;
                var span = _sourceSpan[_startNext.._endNext];
                foreach (var separator in _separators) {
                    if (string.IsNullOrEmpty(separator)) {
                        continue;
                    }
                    var sepIndex = span.IndexOf(separator);
                    if (sepIndex == -1) {
                        continue;
                    }
                    if (separatorIndex == -1 || sepIndex < separatorIndex) {
                        separatorIndex = sepIndex;
                        separatorLength = separator.Length;
                    }
                }
                break;

            case SplitMode.EmptySequence:
                separatorIndex = -1;
                separatorLength = 1;
                break;

            default:
                Debug.Assert(_splitMode == SplitMode.SearchValues, $"Unknown split mode: {_splitMode}");
                separatorIndex = _sourceSpan[_startNext.._endNext].IndexOfAny(_searchValues);
                separatorLength = 1;
                break;
        }
        _startCurrent = _startNext;
        if (separatorIndex >= 0) {
            _endCurrent = _startCurrent + separatorIndex;
            _startNext = _endCurrent + separatorLength;
        } else {
            _startNext = _endCurrent = _endNext;

            // Set _splitMode to None so that subsequent MoveNext calls will return false.
            _splitMode = SplitMode.None;
        }
        return true;
    }

    private bool MoveBackward()
    {
        // Search for the next separator index.
        int separatorIndex, separatorLength;
        switch (_splitMode) {
            case SplitMode.None:
                return false;

            case SplitMode.SingleElement:
                separatorIndex = _sourceSpan[_startNext.._endNext].LastIndexOf(_chSeparator);
                separatorLength = 1;
                break;

            case SplitMode.Any:
                separatorIndex = _sourceSpan[_startNext.._endNext].LastIndexOfAny(_chSeparators);
                separatorLength = 1;
                break;

            case SplitMode.Sequence:
                separatorIndex = _sourceSpan[_startNext.._endNext].LastIndexOf(_chSeparators);
                separatorLength = _chSeparators.Length;
                break;

            case SplitMode.MultiSequence:
                separatorIndex = -1;
                separatorLength = 1;
                var span = _sourceSpan[_startNext.._endNext];
                foreach (var separator in _separators) {
                    if (string.IsNullOrEmpty(separator)) {
                        continue;
                    }
                    var sepIndex = span.LastIndexOf(separator);
                    if (sepIndex == -1) {
                        continue;
                    }
                    if (separatorIndex == -1 || sepIndex > separatorIndex + separatorLength - 1) {
                        separatorIndex = sepIndex;
                        separatorLength = separator.Length;
                    }
                }
                break;

            case SplitMode.EmptySequence:
                separatorIndex = -1;
                separatorLength = 1;
                break;

            default:
                Debug.Assert(_splitMode == SplitMode.SearchValues, $"Unknown split mode: {_splitMode}");
                separatorIndex = _sourceSpan[_startNext.._endNext].LastIndexOfAny(_searchValues);
                separatorLength = 1;
                break;
        }
        _endCurrent = _endNext;
        if (separatorIndex >= 0) {
            _startCurrent = _startNext + separatorIndex + separatorLength;
            _endNext = _startNext + separatorIndex;
        } else {
            _startCurrent = _startNext;
            _endNext = _startNext - 1;
            // Set _splitMode to None so that subsequent MoveNext calls will return false.
            _splitMode = SplitMode.None;
        }
        return true;
    }
}
