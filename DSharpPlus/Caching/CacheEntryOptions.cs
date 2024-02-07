namespace DSharpPlus.Caching;

using System;
using Microsoft.Extensions.Caching.Memory;

public class CacheEntryOptions
{
    /// <summary>
    /// Sliding expiration for the cache entry
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public TimeSpan? SlidingExpiration { get; set; }
    

    /// <summary>
    /// Absolute expiration for the cache entry relative to creation
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    
    public TimeSpan? AbsoluteExpirationRelativeToCreation { get; set; }
    

    internal MemoryCacheEntryOptions ToMemoryCacheEntryOptions()
    {
        if (this.AbsoluteExpirationRelativeToCreation is null && this.SlidingExpiration is null)
        {
            throw new ArgumentException("Either SlidingExpiration or AbsoluteExpirationRelativeToCreation must be set");
        }

        if 
        (
            this.AbsoluteExpirationRelativeToCreation is not null &&
            this.SlidingExpiration is not null &&
            this.AbsoluteExpirationRelativeToCreation < this.SlidingExpiration
        )
        {
            throw new ArgumentException("AbsoluteExpirationRelativeToCreation must be greater than SlidingExpiration");
        }
        
        MemoryCacheEntryOptions options = new();
        
        if (this.AbsoluteExpirationRelativeToCreation is not null)
        {
            options.AbsoluteExpirationRelativeToNow = this.AbsoluteExpirationRelativeToCreation;
        }
        
        if (this.SlidingExpiration is not null)
        {
            options.SlidingExpiration = this.SlidingExpiration;
        }

        return options;
    }
}
