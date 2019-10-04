using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace DSharpPlus
{
    /// <summary>
    /// Read-only view of a given <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// This type exists because <see cref="ConcurrentDictionary{TKey,TValue}"/> is not an
    /// <see cref="IReadOnlyDictionary{TKey,TValue}"/> in .NET Standard 1.1.
    /// </remarks>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    internal readonly struct ReadOnlyConcurrentDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _underlyingDict;

        /// <summary>
        /// Creates a new read-only view of the given dictionary.
        /// </summary>
        /// <param name="underlyingDict">Dictionary to create a view over.</param>
        public ReadOnlyConcurrentDictionary(ConcurrentDictionary<TKey, TValue> underlyingDict)
        {
            this._underlyingDict = underlyingDict;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this._underlyingDict.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this._underlyingDict).GetEnumerator();

        public int Count => this._underlyingDict.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => this._underlyingDict.ContainsKey(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) => this._underlyingDict.TryGetValue(key, out value);

        public TValue this[TKey key] => this._underlyingDict[key];

        public IEnumerable<TKey> Keys => this._underlyingDict.Keys;

        public IEnumerable<TValue> Values => this._underlyingDict.Values;
    }
}