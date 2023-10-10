namespace DSharpPlus.Cache;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

public class DiscordMemoryCache : IDiscordCache
{
    private MemoryCache _cache = new(new MemoryCacheOptions());

    private Dictionary<Type, MemoryCacheEntryOptions> MemoryCacheEntryOptions;
    private Dictionary<Type, Func<object, ICacheKey>> KeyFunctions;

    public DiscordMemoryCache(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        MemoryCacheEntryOptions = configuration.MemoryCacheEntryOptions;
        KeyFunctions = configuration.KeyFunctions;
    }

    public ValueTask Add<T>(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        if (KeyFunctions.TryGetValue(typeof(T), out Func<object, ICacheKey>? keyFunction))
        {
            if (MemoryCacheEntryOptions.TryGetValue(typeof(T), out MemoryCacheEntryOptions? cacheEntryOptions))
            {
                ICacheKey key = keyFunction(entity);
                _cache.Set(key, entity, cacheEntryOptions);
                return default;
            }
            else
            {
                throw new ArgumentException($"Missing MemoryCacheEntryOptions for Type {nameof(T)}");
            }
        }
        else
        {
            throw new ArgumentException($"Missing KeyFunction for Type {nameof(T)}");
        }
    }

    public ValueTask Remove(ICacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        this._cache.Remove(key);
        return default;
    }

    public ValueTask<bool> TryGet<T>(ICacheKey key, out T? entity)
    {
        ArgumentNullException.ThrowIfNull(key);

        entity = this._cache.Get<T>(key);
        return new ValueTask<bool>(entity is not null);
    }

    public bool Validate(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        foreach (Type neededType in IDiscordCache.NeededTypes)
        {
            if (!MemoryCacheEntryOptions.ContainsKey(neededType))
            {
                throw new ArgumentException($"Missing MemoryCacheEntryOptions for needed Type {neededType}");
            }
            if (!configuration.KeyFunctions.ContainsKey(neededType))
            {
                throw new ArgumentException($"Missing KeyFunction for needed Type {neededType}");
            }
        }

        foreach (MemoryCacheEntryOptions entryOptions in configuration.MemoryCacheEntryOptions.Values)
        {
            if (entryOptions.Size.HasValue)
            {
                throw new ArgumentException("Cache dont support limiting by Size");
            }
        }
        return true;
    }
}
