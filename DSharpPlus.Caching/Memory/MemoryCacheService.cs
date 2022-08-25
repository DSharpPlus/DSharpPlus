using System;
using System.Threading.Tasks;

using DSharpPlus.Caching.Abstractions;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Caching.Memory
{
    /// <summary>
    /// An implementation of <see cref="ICacheService"/> relying on a memory cache.
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly MemoryCacheOptions _options;
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IOptions<MemoryCacheOptions> options, IMemoryCache cache)
        {
            _options = options.Value;
            _cache = cache;
        }

        
        /// <inheritdoc/>
        public ValueTask CacheAsync<TItem>(string key, TItem value)
        {
            TimeSpan absolute = _options.GetAbsoluteExpiration(typeof(TItem));

            TimeSpan sliding = _options.GetSlidingExpiration(typeof(TItem));

            _cache.CreateEntry(key)
                .SetValue(value)
                .SetAbsoluteExpiration(absolute)
                .SetSlidingExpiration(sliding);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CacheAsync<TItem>(BaseCacheEntry entry)
        {
            TimeSpan? absolute = null, sliding = null;
            PostEvictionDelegate? postEviction = null;

            if(entry is MemoryCacheEntry memoryCacheEntry)
            {
                absolute = memoryCacheEntry.AbsoluteExpiration;
                sliding = memoryCacheEntry.SlidingExpiration;
                postEviction = memoryCacheEntry.PostEvictionCallback;
            }

            absolute ??= _options.GetAbsoluteExpiration(typeof(TItem));

            sliding ??= _options.GetSlidingExpiration(typeof(TItem));

            ICacheEntry cacheEntry = _cache.CreateEntry(entry.Key)
                .SetValue(entry.Value)
                .SetAbsoluteExpiration(absolute.Value)
                .SetSlidingExpiration(sliding.Value);

            if(postEviction is not null)
            {
                cacheEntry.RegisterPostEvictionCallback(postEviction);
            }

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Creates a new ICacheEntry, applicable only to IMemoryCache, and returns it for more advanced operations.
        /// </summary>
        /// <param name="key">The cache key this entry will utilize.</param>
        public ValueTask<ICacheEntry> CacheAsync(string key) 
            => ValueTask.FromResult(_cache.CreateEntry(key));

        /// <inheritdoc/>
        public ValueTask<TItem?> TryGetAsync<TItem>(string key)
            => ValueTask.FromResult(_cache.TryGetValue(key, out TItem value) ? value : default);

        /// <inheritdoc/>
        public ValueTask<TItem?> RemoveAsync<TItem>(string key)
        {
            ValueTask<TItem?> value = TryGetAsync<TItem>(key);

            _cache.Remove(key);

            return value;
        }
    }
}
