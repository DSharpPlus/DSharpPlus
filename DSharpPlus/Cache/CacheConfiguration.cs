namespace DSharpPlus.Cache;

using System;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Configuration for the cache how long to keep items in the cache
/// </summary>
/// <remarks>The default implementation of <see cref="IDiscordCache"/> only supports timebased expiration </remarks>
public record CacheConfiguration
{
    public MemoryCacheEntryOptions Guild { get; set; }
    public MemoryCacheEntryOptions Channel { get; set; }
    public MemoryCacheEntryOptions Thread { get; set; }
    public MemoryCacheEntryOptions User { get; set; }
    public MemoryCacheEntryOptions Message { get; set; }
    public MemoryCacheEntryOptions Member { get; set; }
}
