using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace DSharpPlus
{
    // Read-only ConcurrentDictionary wrapper for Standard 1.1 but is very low-overhead so might as well use it
    // everywhere
    internal readonly struct ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _implementation;

        public ReadOnlyDictionaryWrapper(ConcurrentDictionary<TKey, TValue> implementation)
        {
            _implementation = implementation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _implementation.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _implementation).GetEnumerator();

        public int Count => _implementation.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => _implementation.ContainsKey(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) => _implementation.TryGetValue(key, out value);

        public TValue this[TKey key] => _implementation[key];

        public IEnumerable<TKey> Keys => _implementation.Keys;

        public IEnumerable<TValue> Values => _implementation.Values;
    }
}