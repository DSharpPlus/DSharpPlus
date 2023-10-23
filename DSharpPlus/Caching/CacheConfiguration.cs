namespace DSharpPlus.Caching;

using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

/// <summary>
/// Configuration for the cache how long to keep items in the cache. This configuration is only used by <see cref="DiscordMemoryCache"/> and is ignored if you provide your own implementation of <see cref="IDiscordCache"/>.
/// </summary>
/// <remarks>The default implementation of <see cref="IDiscordCache"/> only supports timebased expiration </remarks>
public record CacheConfiguration
{
    public Dictionary<Type, CacheEntryOptions> MemoryCacheEntryOptions { get; set; } = new()
    {
        {typeof(DiscordGuild), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordChannel), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordMessage), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordMember), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}},
        {typeof(DiscordUser), new CacheEntryOptions(){SlidingExpiration = TimeSpan.FromMinutes(60)}}
    };
}
