using System;
using System.Collections.Generic;

namespace DSharpPlus.Caching.Abstractions
{
    /// <summary>
    /// Represents a base option class for all cache options.
    /// </summary>
    public abstract class AbstractCacheOptions
    {
        /// <summary>
        /// Specifies a default absolute expiration for all types without a specified expiration time.
        /// </summary>
        public TimeSpan DefaultAbsoluteExpiration { get; set; }

        /// <summary>
        /// Specifies a default sliding expiration for all types without a specified expiration time.
        /// </summary>
        public TimeSpan DefaultSlidingExpiration { get; set; }

        /// <summary>
        /// Stores all absolute expirations for concrete types.
        /// </summary>
        public Dictionary<Type, TimeSpan> AbsoluteExpirations { get; set; } = new();

        /// <summary>
        /// Stores all sliding expirations for concrete types.
        /// </summary>
        public Dictionary<Type, TimeSpan> SlidingExpirations { get; set; } = new();

        /// <summary>
        /// Sets the absolute expiration for the specified type.
        /// </summary>
        /// <remarks>
        /// This method distinguishes between interfaces and concrete types in its implementation.
        /// </remarks>
        /// <typeparam name="T">The type registered to the cache.</typeparam>
        /// <param name="time">The absolute expiration time for this type.</param>
        /// <returns>The cache options object for chaining.</returns>
        public AbstractCacheOptions SetAbsoluteExpiration<T>(TimeSpan time)
        {
            AbsoluteExpirations[typeof(T)] = time;

            return this;
        }

        /// <summary>
        /// Sets the sliding expiration for the specified type.
        /// </summary>
        /// <remarks>
        /// This method distinguishes between interfaces and concrete types in its implementation.
        /// </remarks>
        /// <typeparam name="T">The type registered to the cache.</typeparam>
        /// <param name="time">The sliding expiration time for this type.</param>
        /// <returns>The cache options object for chaining.</returns>
        public AbstractCacheOptions SetSlidingExpiration<T>(TimeSpan time)
        {
            SlidingExpirations[typeof(T)] = time;

            return this;
        }

        // helper methods to fetch expirations

        internal TimeSpan GetAbsoluteExpiration(Type type)
            => AbsoluteExpirations.ContainsKey(type)
                ? AbsoluteExpirations[type]
                : DefaultAbsoluteExpiration;

        internal TimeSpan GetSlidingExpiration(Type type)
            => SlidingExpirations.ContainsKey(type)
                ? SlidingExpirations[type]
                : DefaultSlidingExpiration;
    }
}
