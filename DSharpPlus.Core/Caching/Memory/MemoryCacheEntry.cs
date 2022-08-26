using System;

using DSharpPlus.Core.Caching.Abstractions;

using Microsoft.Extensions.Caching.Memory;

namespace DSharpPlus.Core.Caching.Memory
{
    /// <summary>
    /// Represents an entry in a <see cref="MemoryCacheService"/>
    /// </summary>
    public record MemoryCacheEntry : BaseCacheEntry
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
