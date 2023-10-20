namespace DSharpPlus.Cache;

using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Configuration for the cache how long to keep items in the cache
/// </summary>
/// <remarks>The default implementation of <see cref="IDiscordCache"/> only supports timebased expiration </remarks>
public record CacheConfiguration
{
    public Dictionary<Type, CacheEntryOptions> MemoryCacheEntryOptions { get; set; } = new Dictionary<Type, CacheEntryOptions>()
    {
        {typeof(DiscordGuild), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordChannel), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordMessage), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordMember), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordUser), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}}
    };
}
