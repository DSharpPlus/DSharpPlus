using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DSharpPlus;

/// <summary>
/// Read-only view of a given <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
internal readonly struct ReadOnlyConcurrentDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    private readonly ConcurrentDictionary<TKey, TValue> underlyingDict;

    /// <summary>
    /// Creates a new read-only view of the given dictionary.
    /// </summary>
    /// <param name="underlyingDict">Dictionary to create a view over.</param>
    public ReadOnlyConcurrentDictionary(ConcurrentDictionary<TKey, TValue> underlyingDict) => this.underlyingDict = underlyingDict;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => this.underlyingDict.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.underlyingDict).GetEnumerator();

    public int Count => this.underlyingDict.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) => this.underlyingDict.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, out TValue value) => this.underlyingDict.TryGetValue(key, out value);

    public TValue this[TKey key] => this.underlyingDict[key];

    public IEnumerable<TKey> Keys => this.underlyingDict.Keys;

    public IEnumerable<TValue> Values => this.underlyingDict.Values;
}
