using System.Runtime.CompilerServices;

namespace RvB.Linq;

public static partial class IterableExtensions
{
    public static Iterable<Take<TIterator, TSource>, TSource> Take<TIterator, TSource>(this Iterable<TIterator, TSource> source, int count)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(new(source._iterator, count));
    }

    public static Iterable<TakeSkip<TIterator, TSource>, TSource> Skip<TIterator, TSource>(this Iterable<Take<TIterator, TSource>, TSource> source, int count)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(source._iterator.Skip(count));
    }

    public static Iterable<TakeSkip<TIterator, TSource>, TSource> Skip<TIterator, TSource>(this Iterable<TakeSkip<TIterator, TSource>, TSource> source, int count)
        where TIterator : struct, IIterator<TSource>, allows ref struct
    {
        return new(source._iterator.Skip(count));
    }
}

public ref struct Take<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    internal TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly int _takeCount;
    private int _taken;

    public Take(TIterator source, int count)
    {
        _source = source;
        _takeCount = int.Max(0, count);
    }

    public readonly bool TryGetCount(out int count)
    {
        if (_source.TryGetCount(out count)) {
            count = int.Min(count, _takeCount);
            return true;
        }
        count = 0;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_source.TryGetCount(out var count)) {
            var offset = index.GetOffset(count);
            if (offset >= 0 && offset < _takeCount) {
                return _source.TryGet(offset, out item);
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (_taken < _takeCount) {
            if (_source.TryGetNext(out current)) {
                _taken += 1;
                return true;
            }
            _taken = _takeCount;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    public void Dispose() => _source.Dispose();

    internal TakeSkip<TIterator, TSource> Skip(int skipCount) => new(_source, _takeCount, skipCount);
}

public ref struct TakeSkip<TIterator, TSource> : IIterator<TSource>
    where TIterator : struct, IIterator<TSource>, allows ref struct
{
#pragma warning disable IDE0044 // Add readonly modifier
    private TIterator _source;
#pragma warning restore IDE0044 // Add readonly modifier
    private readonly int _takeCount;
    private readonly int _skipCount;
    private int _taken;
    private int _skipped;
    private bool _reachedTakeLimit;

    public TakeSkip(TIterator source, int takeCount, int skipCount)
    {
        _source = source;
        _takeCount = Math.Max(0, takeCount);
        _skipCount = Math.Max(0, skipCount);
    }

    public bool TryGetCount(out int count)
    {
        if (_source.TryGetCount(out count)) {
            // Apply take limit first
            count = Math.Min(count, _takeCount);
            // Then apply skip
            count = Math.Max(0, count - _skipCount);
            return true;
        }
        count = default;
        return false;
    }

    public readonly bool TryGet(Index index, out TSource item)
    {
        if (_source.TryGetCount(out var sourceCount)) {
            // First limit by take
            var takenCount = Math.Min(sourceCount, _takeCount);
            // Then apply skip
            var skipCount = Math.Min(takenCount, _skipCount);
            var count = takenCount - skipCount;
            if (count > 0) {
                // Calculate offset within the resulting sequence
                var offsetInResult = index.GetOffset(count);
                if ((uint)offsetInResult < (uint)count) {
                    // Calculate the source offset (within the take window, then skip)
                    var sourceOffset = skipCount + offsetInResult;
                    return _source.TryGet(sourceOffset, out item);
                }
            }
        }
        Unsafe.SkipInit(out item);
        return false;
    }

    public bool TryGetNext(out TSource current)
    {
        if (IsResultEmpty()) {
            Unsafe.SkipInit(out current);
            return false;
        }
        // Once we've reached the take limit, we're done
        if (_reachedTakeLimit) {
            Unsafe.SkipInit(out current);
            return false;
        }
        // Skip elements that have been emitted from the take portion
        while (_skipped < _skipCount) {
            // First ensure we haven't exceeded the take limit
            if (_taken >= _takeCount) {
                _reachedTakeLimit = true;
                Unsafe.SkipInit(out current);
                return false;
            }
            // Get next element from source
            if (!_source.TryGetNext(out var _)) {
                Unsafe.SkipInit(out current);
                return false;
            }
            _taken += 1;

            // If we've reached the take limit before finishing skipping,
            // we won't be able to return any elements
            if (_taken >= _takeCount) {
                _reachedTakeLimit = true;
                Unsafe.SkipInit(out current);
                return false;
            }
            _skipped += 1;
        }
        // Check if we've reached the take limit
        if (_taken >= _takeCount) {
            _reachedTakeLimit = true;
            Unsafe.SkipInit(out current);
            return false;
        }
        // Return elements after taking and skipping
        if (_source.TryGetNext(out current)) {
            _taken++;
            return true;
        }
        Unsafe.SkipInit(out current);
        return false;
    }

    private readonly bool IsResultEmpty()
    {
        if (_takeCount == 0) {
            return true;
        }
        if (_skipCount >= _takeCount) {
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        _source.Dispose();
    }

    // Optimize
    internal readonly TakeSkip<TIterator, TSource> Skip(int count)
    {
        if (count <= 0) {
            return this; // no changes.
        }
        // check overflow
        int newSkipCount;
        if (count > 0 && _skipCount > int.MaxValue - count) {
            newSkipCount = int.MaxValue;
        } else {
            newSkipCount = _skipCount + count;
        }
        return new TakeSkip<TIterator, TSource>(_source, _takeCount, newSkipCount);
    }
}
