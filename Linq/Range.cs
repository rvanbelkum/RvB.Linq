namespace RvB.Linq;

public static partial class Iterable
{
    public static Iterable<Range<T>, T> Range<T>(T start, int count)
        where T : INumber<T>, IMinMaxValue<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        if (count > 0) {
            try {
                checked { _ = start + T.CreateChecked(count) - T.One; }
            } catch {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }
        return new(new(start, count, T.One));
    }

    public static Iterable<Range<T>, T> Range<T>(T start, int count, T step)
        where T : INumber<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        if (count > 0) {
            try {
                checked { _ = start + step * (T.CreateChecked(count) - T.One); }
            } catch {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }
        return new(new(start, count, step));
    }
}

public ref struct Range<T> : IIterator<T>
    where T : INumber<T>
{
    private readonly T _step;
    private T _startValue;
    private int _count;

    public Range(T start, int count, T step)
    {
        _step = step;
        _count = count;
        _startValue = start;
    }

    public readonly bool TryGetCount(out int count)
    {
        count = _count;
        return true;
    }

    public readonly bool TryGet(Index index, out T item)
    {
        var offset = index.GetOffset(_count);
        item = _startValue + T.CreateChecked(offset) * _step;
        return true;
    }

    public bool TryGetNext(out T current)
    {
        if (_count > 0) {
            current = _startValue;
            _startValue += _step;
            _count -= 1;
            return true;
        }
        current = default!;
        return false;
    }

    public readonly void Dispose() { }
}
