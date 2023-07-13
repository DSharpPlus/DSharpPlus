#pragma warning disable IDE0073 // don't insert our license header here

// This source file is loosely adapted after the following dotnet/corefxlab file, and is therefore licensed
// under the MIT License:
// https://github.com/dotnet/corefxlab/blob/archive/src/Microsoft.Experimental.Collections/Microsoft/Collections/Extensions/DictionarySlim.cs
// 
// Copyright (c) .NET Foundation and Contributors
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DSharpPlus.Collections;

/// <summary>
/// A lightweight dictionary with three principal differences compared to <see cref="Dictionary{TKey, TValue}"/>.
/// <br/> <br/>
/// 1. It is possible to get or add in a single lookup. For values that are value types, this also
/// saves a copy of the value. <br/>
/// 2. It assumes it is cheap to equate values. <br/>
/// 3. It assumes the keys implement <see cref="IEquatable{T}"/> and that they are cheap and sufficient.
/// </summary>
/// <remarks>
/// This avoids having to do separate lookups for get-or-add scenarios; it can save space by not
/// storing hash codes, save time by skipping potentially expensive hash code calculations for certain types
/// of common keys, as well as avoid storing a comparer and avoid the - likely virtual - call to a comparer.
/// Additionally, this class is a sealed class, making it trivial to devirtualize calls to it.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(DictionarySlimDebugView<,>))]
#if !NETSTANDARD
[SkipLocalsInit]
#endif
public sealed class DictionarySlim<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    where TKey : IEquatable<TKey>
{
    // using this static initialization allows us to initialize further dictionaries without
    // any initial allocations. instead, we allocate once per monomorphized generic instantiation,
    // plus one time for the canonicalized reference type generic instantiation
    // DSharpPlus.Collections.DictionarySlim<System.__Canon, System.__Canon>.
    // The first addition will cause a resize, replacing this with a real array.
    private static readonly DictionaryEntry[] initialEntries = new DictionaryEntry[1];

    private static readonly int[] sizeOneIntArray = new int[1];

    // zero-based index into entries towards the head of the free chain, -1 means empty
    private int freeList = -1;

    // one-based index into entries, 0 means empty.
    private int[] buckets;
    private DictionaryEntry[] entries;

    /// <inheritdoc/>
    public int Count { get; private set; }

    /// <summary>
    /// Represents an entry to this dictionary, replacing KeyValuePairs.
    /// </summary>
    [DebuggerDisplay("({key}, {value})->{next}")]
    private struct DictionaryEntry
    {
        public TKey key;
        public TValue value;

        // zero-based index of the next entry in this chain; -1 signifies the end of the chain.
        // this also encodes whether this entry is part of the free list by changing the sign and
        // subtracting 3; so, -2 means the end of the free list, -3 means index 0 on the free list,
        // -4 means index 1 on the free list et cetera
        public int next;
    }

    /// <summary>
    /// Constructs a new slim dictionary with default capacity.
    /// </summary>
    public DictionarySlim()
    {
        this.buckets = sizeOneIntArray;
        this.entries = initialEntries;
    }

    /// <summary>
    /// Constructs a new dictionary with at least the specified capacity for entries.
    /// </summary>
    /// <remarks>
    /// This constructor will round the capacity up to the nearest power of two.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if capacity was less than 0.
    /// </exception>
    public DictionarySlim
    (
        int capacity
    )
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException
            (
                nameof(capacity),
                "The initial capacity for this dictionary was too small."
            );
        }

        // ensure a capacity of at least two; because 1 would mean the dummy array.
        if (capacity < 2)
        {
            capacity = 2;
        }

        // round up to the nearest power of two
        int roundedCapacity = 2;

        while (roundedCapacity < capacity)
        {
            roundedCapacity <<= 1;
        }

        this.buckets = new int[roundedCapacity];
        this.entries = new DictionaryEntry[roundedCapacity];
    }

    /// <summary>
    /// Clears this dictionary, invalidating any active references and enumerators.
    /// </summary>
    public void Clear()
    {
        this.Count = 0;
        this.freeList = -1;
        this.buckets = sizeOneIntArray;
        this.entries = initialEntries;
    }

    /// <summary>
    /// Looks for the specified key in the dictionary, returning whether it exists.
    /// </summary>
    public bool ContainsKey
    (
        TKey key
    )
    {
#if !NETSTANDARD
        ArgumentNullException.ThrowIfNull(key);
#else
        if (key is null)
        {
            throw new ArgumentNullException("The supplied key was null.");
        }
#endif
        DictionaryEntry[] entries = this.entries;
        int collisions = 0;

        for
        (
            int i = buckets[key.GetHashCode() & (buckets.Length - 1)] - 1; 
            (uint)i < (uint)entries.Length; 
            i = entries[i].next
        )
        {
            if (key.Equals(entries[i].key))
            {
                return true;
            }

            if(collisions == entries.Length)
            {
                ThrowHelper.ThrowConcurrentOperationsNotSupported();
            }

            collisions++;
        }

        return false;
    }

    /// <summary>
    /// Gets the value if one is present for the given key.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <param name="value">The value if one was found, otherwise <c>default(TValue)</c>.</param>
    /// <returns>Whether the key was found.</returns>
    public bool TryGetValue
    (
        TKey key,

        [MaybeNullWhen(false)]
        out TValue value
    )
    {
#if !NETSTANDARD
        ArgumentNullException.ThrowIfNull(key);
#else
        if (key is null)
        {
            throw new ArgumentNullException("The supplied key was null.");
        }
#endif

        DictionaryEntry[] entries = this.entries;
        int collisions = 0;

        for
        (
            int i = buckets[key.GetHashCode() & (buckets.Length - 1)] - 1;
            (uint)i < (uint)entries.Length;
            i = entries[i].next
        )
        {
            if (key.Equals(entries[i].key))
            {
                value = entries[i].value;
                return true;
            }

            if (collisions == entries.Length)
            {
                ThrowHelper.ThrowConcurrentOperationsNotSupported();
            }

            collisions++;
        }

        value = default;
        return false;
    }

    public ref TValue this[TKey key]
    {
        get
        {
#if !NETSTANDARD
            ArgumentNullException.ThrowIfNull(key);
#else
        if (key is null)
        {
            throw new ArgumentNullException("The supplied key was null.");
        }
#endif

            DictionaryEntry[] entries = this.entries;
            int collisions = 0;

            for
            (
                int i = buckets[key.GetHashCode() & (buckets.Length - 1)] - 1;
                (uint)i < (uint)entries.Length;
                i = entries[i].next
            )
            {
                if (key.Equals(entries[i].key))
                {
                    return ref entries[i].value;
                }

                if (collisions == entries.Length)
                {
                    ThrowHelper.ThrowConcurrentOperationsNotSupported();
                }

                collisions++;
            }

            ThrowHelper.ThrowValueNotFound();

            // this is unreachable, but roslyn disagrees; so we need to have a dummy return
            return ref Unsafe.NullRef<TValue>();
        }
    }

    /// <summary>
    /// Removes the entry with the specified key, if one was present.
    /// </summary>
    /// <returns>true if the key was present and an entry was removed.</returns>
    public bool Remove
    (
        TKey key
    )
    {
#if !NETSTANDARD
        ArgumentNullException.ThrowIfNull(key);
#else
        if (key is null)
        {
            throw new ArgumentNullException("The supplied key was null.");
        }
#endif

        DictionaryEntry[] entries = this.entries;
        int bucketIndex = key.GetHashCode() & (buckets.Length - 1);
        int entryIndex = buckets[bucketIndex] - 1;

        int lastIndex = -1;
        int collisions = 0;

        while (entryIndex != -1)
        {
            DictionaryEntry candidate = entries[entryIndex];
            if (candidate.key.Equals(key))
            {
                if (lastIndex != -1)
                {   
                    // fix the preceding element in the chain to point to the correct next element, if any
                    entries[lastIndex].next = candidate.next;
                }
                else
                {   
                    // fix the bucket to the new head, if any
                    buckets[bucketIndex] = candidate.next + 1;
                }

                entries[entryIndex] = default;

                // this is the new head of the free list
                entries[entryIndex].next = -3 - freeList;
                freeList = entryIndex;

                Count--;
                return true;
            }

            lastIndex = entryIndex;
            entryIndex = candidate.next;

            if (collisions == entries.Length)
            {
                // The chain of entries forms a loop; which means a concurrent update has happened.
                // Break out of the loop and throw, rather than looping forever.
                ThrowHelper.ThrowConcurrentOperationsNotSupported();
            }

            collisions++;
        }

        return false;
    }

    // Not safe for concurrent _reads_ (at least, if either of them add)
    // For concurrent reads, prefer TryGetValue(key, out value)
    /// <summary>
    /// Gets the value for the specified key, or, if the key is not present,
    /// adds an entry and returns the value by ref. This makes it possible to
    /// add or update a value in a single look up operation.
    /// </summary>
    /// <param name="key">The key to look for.</param>
    /// <returns>A reference to the new or existing value.</returns>
    public ref TValue GetOrAddValueRef(TKey key)
    {
#if !NETSTANDARD
        ArgumentNullException.ThrowIfNull(key);
#else
        if (key is null)
        {
            throw new ArgumentNullException("The supplied key was null.");
        }
#endif

        DictionaryEntry[] entries = this.entries;

        int collisions = 0;
        int bucketIndex = key.GetHashCode() & (this.buckets.Length - 1);

        for 
        (
            int i = this.buckets[bucketIndex] - 1;
            (uint)i < (uint)entries.Length; 
            i = entries[i].next
        )
        {
            if (key.Equals(entries[i].key))
            {
                return ref entries[i].value;
            }

            if (collisions == entries.Length)
            {
                // The chain of entries forms a loop; which means a concurrent update has happened.
                // Break out of the loop and throw, rather than looping forever.
                ThrowHelper.ThrowConcurrentOperationsNotSupported();
            }

            collisions++;
        }

        return ref AddKey(key, bucketIndex);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private ref TValue AddKey(TKey key, int bucketIndex)
    {
        DictionaryEntry[] entries = this.entries;

        int entryIndex;

        if (this.freeList != -1)
        {
            entryIndex = this.freeList;
            this.freeList = -3 - entries[this.freeList].next;
        }
        else
        {
            if (this.Count == entries.Length || entries.Length == 1)
            {
                entries = this.Resize();
                bucketIndex = key.GetHashCode() & (this.buckets.Length - 1);
                // entry indexes were not changed by Resize
            }

            entryIndex = this.Count;
        }

        entries[entryIndex].key = key;
        entries[entryIndex].next = this.buckets[bucketIndex] - 1;
        this.buckets[bucketIndex] = entryIndex + 1;

        this.Count++;

        return ref entries[entryIndex].value;
    }

    private DictionaryEntry[] Resize()
    {
        // We only copy _count, so if it's longer we will miss some
        // the comparison with 1 is made to catch the initial array
        Debug.Assert(this.entries.Length == this.Count || this.entries.Length == 1);

        int count = this.Count;

        // for net8.0 targets we could consider implementing this for different resize strategies
        // a linearly increasing slim dictionary would be useful.
        int newSize = this.entries.Length * 2;

        if ((uint)newSize > int.MaxValue)
        {
            ThrowHelper.ThrowCapacityIntMaxValueExceeded();
        }

        DictionaryEntry[] entries = new DictionaryEntry[newSize];
        Array.Copy(this.entries, 0, entries, 0, count);

        int[] newBuckets = new int[entries.Length];

        while (count-- > 0)
        {
            int bucketIndex = entries[count].key.GetHashCode() & (newBuckets.Length - 1);
            entries[count].next = newBuckets[bucketIndex] - 1;
            newBuckets[bucketIndex] = count + 1;
        }

        this.buckets = newBuckets;
        this.entries = entries;

        return entries;
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() 
        => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() 
        => new Enumerator(this);

    // avoid boxing enumerators if not necessary
    public Enumerator GetEnumerator() => new(this);

    /// <summary>
    /// The enumerator struct for this dictionary type.
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly DictionarySlim<TKey, TValue> dictionary;
        private int index;
        private int count;
        private KeyValuePair<TKey, TValue> current;

        internal Enumerator
        (
            DictionarySlim<TKey, TValue> dictionary
        )
        {
            this.dictionary = dictionary;
            this.index = 0;
            this.count = dictionary.Count;
            this.current = default;
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (this.count == 0)
            {
                this.current = default;
                return false;
            }

            this.count--;

            while (this.dictionary.entries[this.index].next < -1)
            {
                this.index++;
            }

            this.current = new KeyValuePair<TKey, TValue>
            (
                this.dictionary.entries[this.index].key,
                this.dictionary.entries[this.index++].value
            );

            return true;
        }

        /// <summary>
        /// Get current value
        /// </summary>
        public readonly KeyValuePair<TKey, TValue> Current => this.current;

        readonly object IEnumerator.Current => this.current;

        void IEnumerator.Reset()
        {
            this.index = 0;
            this.count = this.dictionary.Count;
            this.current = default;
        }

        /// <summary>
        /// Dispose the enumerator
        /// </summary>
        public readonly void Dispose() { }
    }
}
