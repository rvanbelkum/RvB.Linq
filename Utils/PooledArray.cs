using System.Buffers;

namespace RvB.Linq.Utils;

internal ref struct PooledArray<T> : IDisposable
{
    private readonly T[] _array;
    private readonly int _size;
    private bool _rented;

    public PooledArray()
    {
        _array = [];
        _rented = false;
    }

    public PooledArray(int size)
    {
        _array = ArrayPool<T>.Shared.Rent(size);
        _size = size;
        _rented = true;
    }

    public readonly int Length => _size;

    public readonly Span<T> AsSpan() => _array.AsSpan(0, _size);

    public void Dispose()
    {
        if (_rented) {
            ArrayPool<T>.Shared.Return(_array);
            _rented = false;
        }
    }

    public override readonly string ToString() => $"PooledArray<{typeof(T).Name}>({_size} / {_array.Length})";
}
