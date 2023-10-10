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
    public Dictionary<Type, MemoryCacheEntryOptions> MemoryCacheEntryOptions = new()
    {
        {typeof(DiscordGuild), new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60))},
        {typeof(DiscordChannel), new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60))},
        {typeof(DiscordMessage), new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60))},
        {typeof(DiscordMember), new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60))},
        {typeof(DiscordUser), new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60))}
    };

    public Dictionary<Type, Func<object, ICacheKey>> KeyFunctions = new()
    {
        {typeof(DiscordChannel), entry => new ChannelCacheKey(((DiscordChannel)entry).Id)},
        {typeof(DiscordGuild), entry => new GuildCacheKey(((DiscordGuild)entry).Id)},
        {typeof(DiscordMessage), entry => new MessageCacheKey(((DiscordMessage)entry).Id)},
        {
            typeof(DiscordMember),
            entry => new MemberCacheKey(((DiscordMember)entry).Id, ((DiscordMember)entry)._guild_id)
        },
        {typeof(DiscordUser), entry => new UserCacheKey(((DiscordUser)entry).Id)}
    };
}
