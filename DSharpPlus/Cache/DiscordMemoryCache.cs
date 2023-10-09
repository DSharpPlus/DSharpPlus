namespace DSharpPlus.Cache;

using System;
using System.Threading.Tasks;
using Entities;
using Microsoft.Extensions.Caching.Memory;

public class DiscordMemoryCache : IDiscordCache
{
    private MemoryCacheEntryOptions _guildOptions;
    private MemoryCacheEntryOptions _channelOptions;
    private MemoryCacheEntryOptions _threadOptions;
    private MemoryCacheEntryOptions _messageOptions;
    private MemoryCacheEntryOptions _userOptions;

    private MemoryCache _cache = new(new MemoryCacheOptions());
    
    public DiscordMemoryCache(CacheConfiguration configuration)
    {
        this._guildOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(configuration.GuildLifetime);
        this._channelOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(configuration.ChannelLifetime);
        this._threadOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(configuration.ThreadLifetime);
        this._messageOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(configuration.MessageLifetime);
        this._userOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(configuration.UserLifetime);
    }


    public ValueTask Add<T>(T entity)
    {
        if (entity is DiscordGuild guild)
        {
            this._cache.Set(new GuildCacheKey(guild.Id), guild, this._guildOptions);
        }
        else if (entity is DiscordChannel channel)
        {
            this._cache.Set(new ChannelCacheKey(channel.Id), channel, this._channelOptions);
        }
        else if (entity is DiscordThreadChannel thread)
        {
            this._cache.Set(new ChannelCacheKey(thread.Id), thread, this._threadOptions);
        }
        else if (entity is DiscordMessage message)
        {
            this._cache.Set(new MessageCacheKey(message.Id), message, this._messageOptions);
        }
        else if (entity is DiscordUser user)
        {
            this._cache.Set(new UserCacheKey(user.Id), user, this._userOptions);
        }
        else if (entity is DiscordMember member)
        {
            this._cache.Set(new MemberCacheKey(member.Id, member.Guild.Id), member, this._userOptions);
        }
        else
        {
            throw new ArgumentException(
                "Invalid type provided. Only DiscordUser, DiscordGuild, DiscordChannel, DiscordThread, and DiscordMessage are supported.");
        }
        return default;
    }

    public ValueTask Remove(ICacheKey key)
    {
        this._cache.Remove(key);
        return default;
    }

    public ValueTask<bool> TryGet<T>(ICacheKey key, out T? entity)
    {
        entity = this._cache.Get<T>(key);
        return new(entity is not null);
    }
}
