namespace DSharpPlus.Caching;

using System.Diagnostics.CodeAnalysis;

public readonly record struct CachedEntity<TKey, TValue> where TValue : class
{
    public TKey Key { get;  }
    
    //private to encourage a more responsible usage
    private TValue? Value { get; }
    
    public CachedEntity(TKey key, TValue? value)
    {
        Key = key;
        Value = value;
    }
    
    public bool HasCachedValue => Value is not null;
    
    public bool TryGetCachedValue([NotNullWhen(true)] out TValue? value)
    {
        value = Value;
        return HasCachedValue;
    }
}
