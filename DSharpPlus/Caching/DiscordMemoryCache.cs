namespace DSharpPlus.Caching;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

public class DiscordMemoryCache : IDiscordCache
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private readonly Dictionary<Type, CacheEntryOptions> _memoryCacheEntryOptions;

    public DiscordMemoryCache(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        this._memoryCacheEntryOptions = configuration.MemoryCacheEntryOptions;
    }

    public Task Add<T>(T entity, ICacheKey key)
    {
        if (entity is null)
        {
            return Task.CompletedTask;
        }
        ArgumentNullException.ThrowIfNull(key);

        KeyValuePair<Type, CacheEntryOptions>? first = new();
        foreach (KeyValuePair<Type, CacheEntryOptions> pair in this._memoryCacheEntryOptions)
        {
            if(!entity.GetType().IsAssignableFrom(pair.Key))
            {
                continue;
            }
            first = pair;
            break;
        }
        CacheEntryOptions? entryOptions = first?.Value;

        if (entryOptions is null)
        {
            return Task.CompletedTask;
        }
        
        this._cache.Set(key, entity, entryOptions.ToMemoryCacheEntryOptions());
        return Task.CompletedTask;
    }

    public ValueTask Remove(ICacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        this._cache.Remove(key);
        return ValueTask.CompletedTask;
    }

    public ValueTask<T?> TryGet<T>(ICacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return ValueTask.FromResult(this._cache.Get<T?>(key));
    }

    public void Dispose() => this._cache.Dispose();
}
