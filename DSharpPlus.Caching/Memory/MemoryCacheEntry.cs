using System;

using DSharpPlus.Caching.Abstractions;

using Microsoft.Extensions.Caching.Memory;

namespace DSharpPlus.Caching.Memory
{
    /// <summary>
    /// Represents an entry in a <see cref="MemoryCacheService"/>
    /// </summary>
    public record MemoryCacheEntry : AbstractCacheEntry
    {
        /// <summary>
        /// Overrides the absolute expiration for this entry.
        /// </summary>
        public TimeSpan? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Overrides the sliding expiration for this entry.
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Sets a post-eviction callback for this entry.
        /// </summary>
        public PostEvictionDelegate? PostEvictionCallback { get; set; }
    }
}
