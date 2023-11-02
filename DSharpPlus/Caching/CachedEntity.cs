namespace DSharpPlus.Caching;


public readonly record struct CachedEntity<TKey, TValue> where TValue : class
{
    TKey Key { get;  }
    TValue? Value { get; }
    
    public CachedEntity(TKey key, TValue? value)
    {
        Key = key;
        Value = value;
    }
    
    public bool HasCachedValue => Value is not null;
    
    public bool TryGetCachedValue(out TValue? value)
    {
        value = Value;
        return HasCachedValue;
    }
}
