namespace DSharpPlus.Cache;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

public class DiscordMemoryCache : IDiscordCache
{
    private MemoryCache _cache = new(new MemoryCacheOptions());

    private Dictionary<Type, CacheEntryOptions> _memoryCacheEntryOptions;

    public DiscordMemoryCache(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        this._memoryCacheEntryOptions = configuration.MemoryCacheEntryOptions;
    }

    public ValueTask Add<T>(T entity, ICacheKey key)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(key);

        CacheEntryOptions? entryOptions = this._memoryCacheEntryOptions
            .Where(x => entity.GetType().IsAssignableFrom(x.Key))
            .FirstOrDefault()
            .Value;

        if (entryOptions is null)
        {
            return default;
        }
        
        _cache.Set(key, entity, entryOptions.ToMemoryCacheEntryOptions());
        return ValueTask.CompletedTask;
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
