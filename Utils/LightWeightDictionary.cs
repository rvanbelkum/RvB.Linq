#define SUPPORT_REMOVE
//#undef SUPPORT_REMOVE
using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RvB.Linq.Utils;

/// <summary>
/// Represents a lightweight, high-performance dictionary optimized for scenarios where memory usage and speed are
/// critical.
/// </summary>
/// <remarks>This dictionary is designed for scenarios where frequent additions and lookups are required, with
/// minimal memory overhead. It uses a custom implementation of hash buckets and entries to achieve high performance.
/// The dictionary is not thread-safe. If multiple threads access the dictionary concurrently, proper
/// synchronization must be implemented.</remarks>
/// <typeparam name="TKey">The type of the keys in the dictionary. Keys must be unique and are compared using the specified or default equality
/// comparer.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
[DebuggerDisplay("Count: {Count}, Buckets: {_usedBuckets} / {_bucketsSize}")]
[CollectionBuilder(typeof(LightWeightDictionaryBuilder), nameof(LightWeightDictionaryBuilder.Create))]
public sealed class LightWeightDictionary<TKey, TValue> : IEnumerable<(TKey Key, TValue Value)>, IDisposable
{
    internal struct Entry
    {
        public uint HashCode;
        public TKey Key;
        public TValue Value;
        public int Next; // -1 is end of chain
    }

    public const int MinimumSize = 16;

    private readonly IEqualityComparer<TKey> _comparer;

    private int[] _buckets;
    private int _bucketsSize;
    private int _usedBuckets;

    private Entry[] _entries;
    private int _entriesSize;
    private int _nextUnusedEntryIndex;
    private int _entriesCount;
#if SUPPORT_REMOVE
    private int _firstFreedEntry;
#endif
    public LightWeightDictionary() : this(MinimumSize, null) { }

    public LightWeightDictionary(int size) : this(size, null) { }

    public LightWeightDictionary(IEqualityComparer<TKey>? comparer) : this(MinimumSize, comparer) { }

    public LightWeightDictionary(int size, IEqualityComparer<TKey>? comparer)
    {
        size = int.Max(size, MinimumSize);
        size = (int)BitOperations.RoundUpToPowerOf2((uint)size);

        _entriesSize = size;
        _entries = ArrayPool<Entry>.Shared.Rent(_entriesSize);
        _nextUnusedEntryIndex = 0;
        _entriesCount = 0;

#if SUPPORT_REMOVE
        // Freed entry indices are represented as "-3 - index". Also for the Next property of a freed Entry.
        // A value of -2 indicates no entry. _firstFreedEntry will always be <= -2
        _firstFreedEntry = -2;
#endif

        _bucketsSize = size;
        _buckets = ArrayPool<int>.Shared.Rent(_bucketsSize);
        _buckets.AsSpan().Clear();
        _usedBuckets = 0;

        _comparer = comparer ?? EqualityComparer<TKey>.Default;
    }

    public int Count => _entriesCount;

    public int HashEfficiency {
        get {
            if (_entriesCount == 0) {
                return 100;
            }
            if (_entriesCount < _bucketsSize) {
                return (int)(100L * _usedBuckets) / _entriesCount;
            }
            return (int)(100L * _usedBuckets) / _bucketsSize;
        }
    }

    /// <summary>
    /// Clears all elements from the collection, resetting it to its initial state.
    /// </summary>
    /// <remarks>After calling this method, the collection will be empty, and all internal counters and
    /// indices will be reset. This operation does not release the underlying storage, so the collection can be reused
    /// without additional allocations.</remarks>
    public void Clear()
    {
        _buckets.AsSpan().Clear();
        _usedBuckets = 0;
        _nextUnusedEntryIndex = 0;
        _entriesCount = 0;
#if SUPPORT_REMOVE
        _firstFreedEntry = -2;
#endif
    }

    /// <summary>
    /// Adds the specified key and value to the collection.
    /// </summary>
    /// <param name="key">The key to add to the collection.</param>
    /// <param name="value">The value associated with the specified key.</param>
    /// <returns><see langword="true"/> if the key and value were successfully added;  otherwise, <see langword="false"/> if the
    /// key already exists in the collection.</returns>
    public bool Add(TKey key, TValue value)
    {
        var hashCode = GetHashCode(key);
        ref var valueFound = ref FindValue(key, hashCode);
        if (!Unsafe.IsNullRef(ref valueFound)) {
            return false;
        }
        _ = AddInternal(key, hashCode, value);
        return true;
    }

    /// <summary>
    /// Gets a reference to the value associated with the specified key, or adds a default value if the key does not
    /// exist.
    /// </summary>
    /// <remarks>This method allows direct manipulation of the value in the collection by returning a
    /// reference.  If the key does not exist, a default value is added to the collection and its reference is
    /// returned.</remarks>
    /// <param name="key">The key of the value to retrieve or add.</param>
    /// <param name="exists">When this method returns, contains <see langword="true"/> if the key exists in the collection;  otherwise, <see
    /// langword="false"/>.</param>
    /// <returns>A reference to the value associated with the specified key. If the key does not exist, a reference to the newly
    /// added default value is returned.</returns>
    public ref TValue GetValueRefOrAddDefault(TKey key, out bool exists)
    {
        var hashCode = GetHashCode(key);
        ref TValue value = ref FindValue(key, hashCode);
        if (!Unsafe.IsNullRef(ref value)) {
            exists = true;
            return ref value;
        }
        exists = false;
        return ref AddInternal(key, hashCode, default!);
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <remarks>This method checks for the presence of the specified key in the dictionary.  The operation is
    /// typically O(1), but performance may vary depending on the implementation  and the quality of the hash function
    /// used.</remarks>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns><see langword="true"/> if the dictionary contains an element with the specified key;  otherwise, <see
    /// langword="false"/>.</returns>
    public bool ContainsKey(TKey key)
    {
        var hashCode = GetHashCode(key);
        ref TValue value = ref FindValue(key, hashCode);
        return !Unsafe.IsNullRef(ref value);
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key.
    /// </summary>
    /// <remarks>This method does not throw an exception if the key is not found. Instead, it returns <see
    /// langword="false"/>  and sets <paramref name="value"/> to its default value.</remarks>
    /// <param name="key">The key whose associated value is to be retrieved.</param>
    /// <param name="value">If the key is found, contains the value associated with the specified key.</param>
    /// <returns><see langword="true"/> if the key was found and the associated value was retrieved; otherwise, <see
    /// langword="false"/>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var hashCode = GetHashCode(key);
        ref var valueFound = ref FindValue(key, hashCode);
        if (!Unsafe.IsNullRef(ref valueFound)) {
            value = valueFound;
            return true;
        }
        value = default;
        return false;
    }

#if SUPPORT_REMOVE
    /// <summary>
    /// Removes the entry with the specified key from the collection.
    /// </summary>
    /// <remarks>This method searches for the specified key in the collection and removes the corresponding
    /// entry  if it exists. If the key is not found, the collection remains unchanged, and the method returns  <see
    /// langword="false"/>.</remarks>
    /// <param name="key">The key of the entry to remove.</param>
    /// <returns><see langword="true"/> if the entry with the specified key was successfully removed;  otherwise, <see
    /// langword="false"/> if the key was not found in the collection.</returns>
    public bool Remove(TKey key)
    {
        var hashCode = GetHashCode(key);
        ref var bucket = ref _buckets[GetBucketIndex(hashCode)];
        var entryIndex = bucket - 1;
        if (entryIndex == -1) {
            return false;
        }
        var comparer = _comparer;
        var entries = _entries;
        var prevEntryIndex = -1;
        do {
            ref var entry = ref entries[entryIndex];
            if (entry.HashCode == hashCode && comparer.Equals(entry.Key, key)) {
                if (prevEntryIndex == -1) {
                    // First entry in the bucket
                    if (entry.Next == -1) {
                        // Only entry in the bucket
                        bucket = 0;
                        _usedBuckets -= 1;
                    } else {
                        bucket = entry.Next + 1;
                    }
                } else {
                    entries[prevEntryIndex].Next = entry.Next;
                }
                entry.Next = _firstFreedEntry;
                _firstFreedEntry = -3 - entryIndex;
                _entriesCount -= 1;
                return true;
            }
            prevEntryIndex = entryIndex;
            entryIndex = entry.Next;
        } while (entryIndex != -1);
        return false;
    }
#endif

    private ref TValue FindValue(TKey key, uint hashCode)
    {
        ref var bucket = ref _buckets[GetBucketIndex(hashCode)];
        var entryIndex = bucket - 1;
        if (entryIndex != -1) {
            var comparer = _comparer;
            var entries = _entries;
            do {
                ref var entry = ref entries[entryIndex];
                if (entry.HashCode == hashCode && comparer.Equals(entry.Key, key)) {
                    return ref entry.Value;
                }
                entryIndex = entry.Next;
            } while (entryIndex != -1);
        }
        return ref Unsafe.NullRef<TValue>();
    }

    private ref TValue AddInternal(TKey key, uint hashCode, TValue value)
    {
        ref var newEntry = ref GetNextEntry(out var entryIndex);
        ref var bucket = ref _buckets[GetBucketIndex(hashCode)];
        newEntry.HashCode = hashCode;
        newEntry.Key = key;
        newEntry.Value = value;
        newEntry.Next = bucket - 1;

        if (bucket == 0) {
            _usedBuckets += 1;
        }
        bucket = entryIndex + 1;
        _entriesCount += 1;
        return ref newEntry.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry GetNextEntry(out int entryIndex)
    {
#if SUPPORT_REMOVE
        if (_firstFreedEntry < -2) {
            entryIndex = -3 - _firstFreedEntry;
            ref var freeEntry = ref _entries[entryIndex];
            _firstFreedEntry = freeEntry.Next;
            return ref freeEntry;
        }
#endif
        if (_nextUnusedEntryIndex == _entriesSize) {
            ResizeElements();
        }
        if (_entriesCount > 3 * _bucketsSize) {
            ResizeBuckets();
        }
        entryIndex = _nextUnusedEntryIndex++;
        return ref _entries[entryIndex];
    }

    public void Dispose()
    {
        if (_bucketsSize > 0) {
            ArrayPool<int>.Shared.Return(_buckets, clearArray: false);
            _bucketsSize = 0;
        }
        if (_entriesSize > 0) {
            ArrayPool<Entry>.Shared.Return(_entries, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<Entry>());
            _entriesSize = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBucketIndex(uint hashCode)
    {
        return (int)(hashCode & (_bucketsSize - 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetHashCode(TKey key)
    {
        return (uint)((key == null) ? 0 : _comparer.GetHashCode(key));
    }

    private void ResizeElements()
    {
        var newEntriesLength = _entriesSize << 1;
        var newEntries = ArrayPool<Entry>.Shared.Rent(newEntriesLength);
        Array.Copy(_entries, newEntries, _entriesSize);
        ArrayPool<Entry>.Shared.Return(_entries, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<Entry>());
        _entries = newEntries;
        _entriesSize = newEntriesLength;
    }

    private void ResizeBuckets()
    {
        ArrayPool<int>.Shared.Return(_buckets, clearArray: false);
        _bucketsSize <<= 2;
        var buckets = ArrayPool<int>.Shared.Rent(_bucketsSize);
        buckets.AsSpan().Clear();

        _usedBuckets = 0;
        for (int i = 0; i < _nextUnusedEntryIndex; i++) {
            ref var entry = ref _entries[i];
            // Items with Next <= -2 are in a deleted state.
            if (entry.Next >= -1) {
                var bucketIndex = GetBucketIndex(entry.HashCode);

                ref var bucket = ref buckets[bucketIndex];
                entry.Next = bucket - 1;
                if (bucket == 0) {
                    _usedBuckets += 1;
                }
                bucket = i + 1;
            }
        }
        _buckets = buckets;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<(TKey Key, TValue Value)> IEnumerable<(TKey Key, TValue Value)>.GetEnumerator()
        => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<(TKey Key, TValue Value)>, IDisposable
    {
        private readonly LightWeightDictionary<TKey, TValue> _dictionary;
        private readonly int _entriesCount;
        private (TKey Key, TValue Value) _current;
        private int _index;

        internal Enumerator(LightWeightDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _entriesCount = dictionary._nextUnusedEntryIndex;
        }

        public readonly (TKey Key, TValue Value) Current => _current;

        readonly object IEnumerator.Current => _current;

        public bool MoveNext()
        {
            while ((uint)_index < (uint)_entriesCount) {
                ref var entry = ref _dictionary._entries[_index++];
                if (entry.Next >= -1) {
                    _current = (entry.Key, entry.Value);
                    return true;
                }
            }
            return false;
        }

        public readonly void Dispose() { }

        public void Reset()
        {
            _index = 0;
        }
    }
}

public class LightWeightDictionaryBuilder
{
    public static LightWeightDictionary<TKey, TValue> Create<TKey, TValue>(ReadOnlySpan<(TKey, TValue)> items)
    {
        var dictionary = new LightWeightDictionary<TKey, TValue>(items.Length);
        foreach (var item in items) {
            dictionary.Add(item.Item1, item.Item2);
        }
        return dictionary;
    }
}
