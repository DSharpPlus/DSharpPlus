using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace DSharpPlus
{
    internal readonly struct ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _implementation;

        public ReadOnlyDictionaryWrapper(ConcurrentDictionary<TKey, TValue> implementation)
        {
            _implementation = implementation;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _implementation.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _implementation).GetEnumerator();

        public int Count => _implementation.Count;

        public bool ContainsKey(TKey key) => _implementation.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _implementation.TryGetValue(key, out value);

        public TValue this[TKey key] => _implementation[key];

        public IEnumerable<TKey> Keys => _implementation.Keys;

        public IEnumerable<TValue> Values => _implementation.Values;
    }
}