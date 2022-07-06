using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DSharpPlus.Caching.Abstractions
{
    /// <summary>
    /// Exposes API to access caches without needing to directly rely on IMemoryCache, IDistributedCache or the likes.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <typeparam name="TItem">Type the item should be treated as.</typeparam>
        /// <param name="key">Cache key for this item.</param>
        /// <param name="value">The item.</param>
        public ValueTask CacheAsync<TItem>(object key, TItem value);

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <typeparam name="TItem">Type the item should be treated as.</typeparam>
        /// <param name="entry">Cache entry data for this item.</param>
        public ValueTask CacheAsync<TItem>(AbstractCacheEntry entry);

        /// <summary>
        /// Gets an item from the cache.
        /// </summary>
        /// <typeparam name="TItem">The type of this item.</typeparam>
        /// <param name="key">The key this item was cached with.</param>
        /// <returns>The item.</returns>
        public ValueTask<bool> TryGetAsync<TItem>(object key,
            [NotNullWhen(true)]
            out TItem value);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The key this item was cached with.</param>
        public ValueTask RemoveAsync(object key);
    }
}
