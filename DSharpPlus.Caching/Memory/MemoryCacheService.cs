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
        public ValueTask CacheAsync<TItem>(object key, TItem value)
        {
            TimeSpan absolute = _options.AbsoluteExpirations.ContainsKey(typeof(TItem))
                ? _options.AbsoluteExpirations[typeof(TItem)]
                : _options.DefaultAbsoluteExpiration;

            TimeSpan sliding = _options.SlidingExpirations.ContainsKey(typeof(TItem))
                ? _options.SlidingExpirations[typeof(TItem)]
                : _options.DefaultSlidingExpiration;

            _cache.CreateEntry(key)
                .SetValue(value)
                .SetAbsoluteExpiration(absolute)
                .SetSlidingExpiration(sliding);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask CacheAsync<TItem>(AbstractCacheEntry entry)
        {
            TimeSpan? absolute = null, sliding = null;
            PostEvictionDelegate? postEviction = null;

            if(entry is MemoryCacheEntry memoryCacheEntry)
            {
                absolute = memoryCacheEntry.AbsoluteExpiration;
                sliding = memoryCacheEntry.SlidingExpiration;
                postEviction = memoryCacheEntry.PostEvictionCallback;
            }

            absolute ??= _options.AbsoluteExpirations.ContainsKey(typeof(TItem))
                ? _options.AbsoluteExpirations[typeof(TItem)]
                : _options.DefaultAbsoluteExpiration;

            sliding ??= _options.SlidingExpirations.ContainsKey(typeof(TItem))
                ? _options.SlidingExpirations[typeof(TItem)]
                : _options.DefaultSlidingExpiration;

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

        /// <inheritdoc/>
        public ValueTask<bool> TryGetAsync<TItem>(object key, out TItem value) 
            => ValueTask.FromResult(_cache.TryGetValue(key, out value));

        /// <inheritdoc/>
        public ValueTask RemoveAsync(object key)
        {
            _cache.Remove(key);

            return ValueTask.CompletedTask;
        }
    }
}
