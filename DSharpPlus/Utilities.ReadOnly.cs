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
        private readonly ConcurrentDictionary<TKey, TValue> _dictionaryImplementation;

        public ReadOnlyDictionaryWrapper(ConcurrentDictionary<TKey, TValue> dictionaryImplementation)
        {
            _dictionaryImplementation = dictionaryImplementation;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionaryImplementation.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _dictionaryImplementation).GetEnumerator();

        public int Count => _dictionaryImplementation.Count;

        public bool ContainsKey(TKey key) => _dictionaryImplementation.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _dictionaryImplementation.TryGetValue(key, out value);

        public TValue this[TKey key] => _dictionaryImplementation[key];

        public IEnumerable<TKey> Keys => _dictionaryImplementation.Keys;

        public IEnumerable<TValue> Values => _dictionaryImplementation.Values;
    }
}