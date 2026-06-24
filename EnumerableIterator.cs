using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RvB.Linq;

internal interface ICollectionIterator<T> : IDisposable
{
    //public bool TryGetCount(ref IteratorSource<T> source, ref InteratorContext context, out int count);

    bool TryGet(ref IteratorSource<T> source, ref InteratorContext context, Index index, out T item);

    bool TryGetNext(ref IteratorSource<T> source, ref InteratorContext context, out T current);

    bool TryGetSpan(ref IteratorSource<T> source, ref InteratorContext context, out ReadOnlySpan<T> span);
}

file sealed class ArrayIterator<T> : ICollectionIterator<T>
{
    public static ArrayIterator<T> Default { get; } = new();

    private ArrayIterator() { }

    //public bool TryGetCount(ref IteratorSource<T> source, ref InteratorContext context, out int count)
    //{
    //    count = source.AsArray.Length;
    //    return true;
    //}

    public bool TryGet(ref IteratorSource<T> source, ref InteratorContext context, Index index, out T item)
    {
        var data = source.AsArray;
        var offset = index.GetOffset(data.Length);
        if ((uint)offset < (uint)data.Length) {
            item = data[offset];
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(ref IteratorSource<T> source, ref InteratorContext context, out T current)
    {
        if (context.Index < context.Count) {
            current = source.AsArray[context.Index++];
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public bool TryGetSpan(ref IteratorSource<T> source, ref InteratorContext context, out ReadOnlySpan<T> span)
    {
        if (context.Index < context.Count) {
            span = source.AsArray[context.Index..context.Count].AsSpan();
            context.Index = context.Count;
            return true;
        }
        Unsafe.SkipInit(out span);
        return false;
    }

    public void Dispose() { }
}

file sealed class ListIterator<T> : ICollectionIterator<T>
{
    public static ListIterator<T> Default { get; } = new();

    private ListIterator() { }

    //public bool TryGetCount(ref IteratorSource<T> source, ref InteratorContext context, out int count)
    //{
    //    count = source.AsList.Count;
    //    return true;
    //}

    public bool TryGet(ref IteratorSource<T> source, ref InteratorContext context, Index index, out T item)
    {
        var data = source.AsList;
        var offset = index.GetOffset(data.Count);
        if ((uint)offset < (uint)data.Count) {
            item = data[offset];
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(ref IteratorSource<T> source, ref InteratorContext context, out T current)
    {
        if (context.Index < context.Count) {
            current = source.AsList[context.Index++];
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public bool TryGetSpan(ref IteratorSource<T> source, ref InteratorContext context, out ReadOnlySpan<T> span)
    {
        if (context.Index < context.Count) {
            span = CollectionsMarshal.AsSpan(source.AsList)[context.Index..context.Count];
            context.Index = context.Count;
            return true;
        }
        Unsafe.SkipInit(out span);
        return false;
    }

    public void Dispose() { }
}

file sealed class IListIterator<T> : ICollectionIterator<T>
{
    public static IListIterator<T> Default { get; } = new();

    private IListIterator() { }

    //public bool TryGetCount(ref IteratorSource<T> source, ref InteratorContext context, out int count)
    //{
    //    count = source.AsIList.Count;
    //    return true;
    //}

    public bool TryGet(ref IteratorSource<T> source, ref InteratorContext context, Index index, out T item)
    {
        var data = source.AsIList;
        var offset = index.GetOffset(data.Count);
        if ((uint)offset < (uint)data.Count) {
            item = data[offset];
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(ref IteratorSource<T> source, ref InteratorContext context, out T current)
    {
        if (context.Index < context.Count) {
            current = source.AsIList[context.Index++];
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public bool TryGetSpan(ref IteratorSource<T> source, ref InteratorContext context, out ReadOnlySpan<T> span)
    {
        Unsafe.SkipInit(out span);
        return false;
    }

    public void Dispose() { }
}

file sealed class IReadOnlyListIterator<T> : ICollectionIterator<T>
{
    public static IReadOnlyListIterator<T> Default { get; } = new();

    private IReadOnlyListIterator() { }

    //public bool TryGetCount(ref IteratorSource<T> source, ref InteratorContext context, out int count)
    //{
    //    count = source.AsIReadOnlyList.Count;
    //    return true;
    //}

    public bool TryGet(ref IteratorSource<T> source, ref InteratorContext context, Index index, out T item)
    {
        var data = source.AsIReadOnlyList;
        var offset = index.GetOffset(data.Count);
        if ((uint)offset < (uint)data.Count) {
            item = data[offset];
            return true;
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(ref IteratorSource<T> source, ref InteratorContext context, out T current)
    {
        if (context.Index < context.Count) {
            current = source.AsIReadOnlyList[context.Index++];
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public bool TryGetSpan(ref IteratorSource<T> source, ref InteratorContext context, out ReadOnlySpan<T> span)
    {
        Unsafe.SkipInit(out span);
        return false;
    }

    public void Dispose() { }
}

file sealed class IEnumerableIterator<T> : ICollectionIterator<T>
{
    private IEnumerator<T>? _enumerator;

    public IEnumerableIterator() { }

    //public bool TryGetCount(ref IteratorSource<T> source, ref InteratorContext context, out int count)
    //{
    //    if (context.Count >= 0) {
    //        count = context.Count;
    //        return true;
    //    }
    //    count = 0;
    //    return false;
    //}

    public bool TryGet(ref IteratorSource<T> source, ref InteratorContext context, Index index, out T item)
    {
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(ref IteratorSource<T> source, ref InteratorContext context, out T current)
    {
        _enumerator ??= source.AsIEnumerable.GetEnumerator();
        if (_enumerator.MoveNext()) {
            current = _enumerator.Current;
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public bool TryGetSpan(ref IteratorSource<T> source, ref InteratorContext context, out ReadOnlySpan<T> span)
    {
        Unsafe.SkipInit(out span);
        return false;
    }

    public void Dispose()
    {
        _enumerator?.Dispose();
        _enumerator = null;
    }
}

internal struct IteratorSource<T>
{
    internal object? Enumerable;

    public readonly T[] AsArray => (Enumerable as T[])!;
    public readonly List<T> AsList => (Enumerable as List<T>)!;
    public readonly IReadOnlyList<T> AsIReadOnlyList => (Enumerable as IReadOnlyList<T>)!;
    public readonly IList<T> AsIList => (Enumerable as IList<T>)!;
    public readonly IEnumerable<T> AsIEnumerable => (Enumerable as IEnumerable<T>)!;
}

internal struct InteratorContext
{
    public int Index;
    public int Count;
}

public ref struct EnumerableIterator<T> : IIterator<T>
{
    private readonly ICollectionIterator<T> _iterator;
    //private readonly IEnumerable<T> _source;
    private IteratorSource<T> _iteratorSource;
    private InteratorContext _context;

    public EnumerableIterator(IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source is T[] array) {
            _iterator = ArrayIterator<T>.Default;
            _context.Count = array.Length;
        } else if (source is List<T> list) {
            _iterator = ListIterator<T>.Default;
            _context.Count = list.Count;
        } else if (source is IReadOnlyList<T> readonlyList) {
            _iterator = IReadOnlyListIterator<T>.Default;
            _context.Count = readonlyList.Count;
        } else if (source is IList<T> ilist) {
            _iterator = IListIterator<T>.Default;
            _context.Count = ilist.Count;
        } else if (source is ICollection<T> collection) {
            _iterator = new IEnumerableIterator<T>();
            _context.Count = collection.Count;
        } else if (source is Queue<T> queue) {
            _iterator = new IEnumerableIterator<T>();
            _context.Count = queue.Count;
        } else if (source is Stack<T> stack) {
            _iterator = new IEnumerableIterator<T>();
            _context.Count = stack.Count;
        } else {
            _iterator = new IEnumerableIterator<T>();
            _context.Count = -1;
        }
        _iteratorSource.Enumerable = source;
        //_source = source;
    }

    public readonly bool TryGetCount(out int count)
    {
        if (_context.Count >= 0) {
            count = _context.Count;
            return true;
        }
        count = 0;
        return false;
    }

    public bool TryGet(Index index, out T item)
    {
        return _iterator.TryGet(ref _iteratorSource, ref _context, index, out item);
    }

    public bool TryGetNext(out T current)
    {
        return _iterator.TryGetNext(ref _iteratorSource, ref _context, out current);
    }

    public void Dispose()
    {
        _context.Index = 0;
        _context.Count = 0;
        _iterator.Dispose();
    }
}
