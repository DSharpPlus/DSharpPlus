namespace DSharpPlus.Caching.Abstractions
{
    /// <summary>
    /// Represents an extendable class for more complex cache entries supported by <see cref="ICacheService"/>.
    /// </summary>
    public record BaseCacheEntry
    {
        /// <summary>
        /// The key used to store this object into cache.
        /// </summary>
        public object Key { get; set; } = null!;

        /// <summary>
        /// The value of this cache entry.
        /// </summary>
        public object Value { get; set; } = null!;
    }
}
