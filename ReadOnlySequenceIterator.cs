using System.Buffers;
using System.Runtime.CompilerServices;

namespace RvB.Linq;

public ref struct ReadOnlySequenceIterator<T> : IIterator<T>
{
    private readonly ReadOnlySequence<T> _sequence;
    private ReadOnlySequence<T>.Enumerator _sequenceEnumerator;
    private ReadOnlySpan<T> _span;
    private int _spanIndex;
    private byte _state;

    public ReadOnlySequenceIterator(ReadOnlySequence<T> sequence)
    {
        _sequence = sequence;
    }

    public readonly bool TryGetCount(out int count)
    {
        if (_sequence.Length <= int.MaxValue) {
            count = (int)_sequence.Length;
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out T item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out T current)
    {
        if (_state == 0) {
            _sequenceEnumerator = _sequence.GetEnumerator();
            _state = 1;
        }
    State1:
        if (_state == 1) {
            if (_sequenceEnumerator.MoveNext()) {
                _span = _sequenceEnumerator.Current.Span;
                _spanIndex = 0;
                _state = 2;
            } else {
                _state = 3;
            }
        }
        if (_state == 2) {
            if ((uint)_spanIndex < (uint)_span.Length) {
                current = _span[_spanIndex++];
                return true;
            }
            _state = 1;
            goto State1;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public readonly bool TryGetSpan(out ReadOnlySpan<T> span)
    {
        Unsafe.SkipInit(out span);
        return false;
    }

    public readonly void Dispose() { }
}
