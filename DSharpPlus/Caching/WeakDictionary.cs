namespace DSharpPlus.Caching;

using System;
using System.Collections.Generic;
using System.Linq;

public class WeakDictionary<TKey, TValue> where TValue : class
{
    private List<KeyValuePair<TKey,WeakReference<TValue>>> _list = new();

    public void Clear() => this._list.Clear();
    
    public void Add(TKey key, TValue value)
    {
        WeakReference<TValue> item = new(value, false);
        KeyValuePair<TKey, WeakReference<TValue>> newPair = new(key, item);
        foreach (KeyValuePair<TKey, WeakReference<TValue>> pair in this._list)
        {
            if (pair.Key != null && pair.Key.Equals(key))
            {
                this._list.Remove(pair);
                this._list.Add(newPair);
                return;
            }
        }
        
        this._list.Add(newPair);
    }

    public bool ContainsKey(TKey key)
    {
        foreach (KeyValuePair<TKey, WeakReference<TValue>> pair in this._list)
        {
            if (pair.Key != null && pair.Key.Equals(key))
            {
                if (pair.Value.TryGetTarget(out _) == false)
                {
                    this._list.Remove(pair);
                    return false;
                }
                return true;
            }
        }

        return false;
    }

    public bool Remove(TKey key)
    {
        foreach (KeyValuePair<TKey, WeakReference<TValue>> pair in this._list)
        {
            if (pair.Key != null && pair.Key.Equals(key))
            {
                this._list.Remove(pair);
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        foreach (KeyValuePair<TKey, WeakReference<TValue>> pair  in this._list)
        {
            if (pair.Key != null && pair.Key.Equals(key))
            {
                if (pair.Value.TryGetTarget(out value) == false)
                {
                    this._list.Remove(pair);
                    return false;
                }
                return true;
            }
        }

        value = null;
        return false;
    }
    
    public IEnumerable<TKey> Keys => this._list.Select(x => x.Key);

    public IEnumerable<TValue> Values => this._list.Select(x =>
        {
            TValue? xValue;
            x.Value.TryGetTarget(out xValue);
            return xValue;
        })
        .Where(x => x is not null)!;
}
