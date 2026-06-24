namespace RvB.Linq;

public interface IIterator<T> : IDisposable
    where T : allows ref struct
{
    bool TryGetCount(out int count);
    bool TryGetNext(out T current);

    bool TryGet(Index index, out T item);
}
