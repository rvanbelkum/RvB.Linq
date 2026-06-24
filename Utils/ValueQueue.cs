using System.Buffers;
using System.Runtime.CompilerServices;

namespace RvB.Linq.Utils;

internal struct ValueQueue<T> : IDisposable
{
    private T[] _items;
    private int _head;
    //private int _tail;
    private int _count;
    private int _capacity;
    private readonly int _maxCapacity;

    public int Count => _count;

    public ValueQueue(int capacity) : this(capacity, int.MaxValue) { }

    public ValueQueue(int capacity, int maxCapacity)
    {
        _capacity = int.Min(maxCapacity, capacity);
        _items = ArrayPool<T>.Shared.Rent(_capacity);
        _head = _count = 0;
        _maxCapacity = maxCapacity;
    }

    public void Enqueue(T item)
    {
        if (_capacity == _count && _capacity < _maxCapacity) {
            Grow();
        }
        _items[(_head + _count) % _capacity] = item;
        if (_count < _capacity) {
            _count += 1;
        } else {
            _head = (_head + 1) % _capacity;
        }
    }

    public T Dequeue()
    {
        if (TryDequeue(out var item)) {
            return item;
        }
        throw new InvalidOperationException("Queue is empty.");
    }

    public bool TryDequeue(out T item)
    {
        if (_count == 0) {
            item = default!;
            return false;
        }
        item = _items[_head];
        _items[_head] = default!;
        _head = (_head + 1) % _capacity;
        _count -= 1;
        return true;
    }

    public T DequeueEnqueue(T item)
    {
        var dequeued = Dequeue();
        Enqueue(item);
        return dequeued;
    }

    private void Grow()
    {
        var newCapacity = int.Min(_maxCapacity, _capacity * 2);
        if (newCapacity <= _capacity) {
            return;
        }
        var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
        if (_count > 0) {
            var tail = (_head + _count) % _capacity;
            if (_head < tail) {
                Array.Copy(_items, _head, newArray, 0, _count);
            } else {
                Array.Copy(_items, _head, newArray, 0, _capacity - _head);
                Array.Copy(_items, 0, newArray, _capacity - _head, tail);
            }
        }
        ArrayPool<T>.Shared.Return(_items, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        _items = newArray;
        _head = 0;
        _capacity = newCapacity;
    }

    public void Dispose()
    {
        if (_items != null) {
            ArrayPool<T>.Shared.Return(_items, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            _items = null!;
        }
    }
}
