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
    private MemoryCacheEntryOptions _memberOptions;
    private MemoryCacheEntryOptions _userOptions;

    private MemoryCache _cache = new(new MemoryCacheOptions());

    public DiscordMemoryCache(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        this._guildOptions = configuration.Guild;
        this._channelOptions = configuration.Channel;
        this._threadOptions = configuration.Thread;
        this._messageOptions = configuration.Message;
        this._userOptions = configuration.User;
        this._memberOptions = configuration.Member;
    }

    public ValueTask Add<T>(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        switch (entity)
        {
            case DiscordGuild guild:
                this._cache.Set(new GuildCacheKey(guild.Id), guild, this._guildOptions);
                break;
            case DiscordThreadChannel thread:
                this._cache.Set(new ChannelCacheKey(thread.Id), thread, this._threadOptions);
                break;
            case DiscordChannel channel:
                this._cache.Set(new ChannelCacheKey(channel.Id), channel, this._channelOptions);
                break;
            case DiscordMessage message:
                this._cache.Set(new MessageCacheKey(message.Id), message, this._messageOptions);
                break;
            case DiscordMember member:
                this._cache.Set(new MemberCacheKey(member.Id, member._guild_id), member, this._userOptions);
                break;
            case DiscordUser user:
                this._cache.Set(new UserCacheKey(user.Id), user, this._userOptions);
                break;
            default:
                throw new ArgumentException(
                    "Invalid type provided. Only DiscordUser, DiscordGuild, DiscordChannel, DiscordThread, and DiscordMessage are supported.");
        }

        return default;
    }

    public ValueTask Remove(ICacheKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        this._cache.Remove(key);
        return default;
    }

    public ValueTask<bool> TryGet<T>(ICacheKey key, out T? entity)
    {
        ArgumentNullException.ThrowIfNull(key);

        entity = this._cache.Get<T>(key);
        return new ValueTask<bool>(entity is not null);
    }

    public bool Validate(CacheConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        if (configuration.Guild.Size.HasValue)
        {
            throw new ArgumentException("Cache dont support limiting by Size" , nameof(configuration.Guild));
        }
        if (configuration.Channel.Size.HasValue)
        {
            throw new ArgumentException("Cache dont support limiting by Size" , nameof(configuration.Channel));
        }
        if (configuration.Thread.Size.HasValue)
        {
            throw new ArgumentException("Cache dont support limiting by Size" , nameof(configuration.Thread));
        }
        if (configuration.User.Size.HasValue)
        {
            throw new ArgumentException("Cache dont support limiting by Size" , nameof(configuration.User));
        }
        if (configuration.Message.Size.HasValue)
        {
            throw new ArgumentException("Cache dont support limiting by Size" , nameof(configuration.Message));
        }
        if (configuration.Member.Size.HasValue)
        {
            throw new ArgumentException("Cache dont support limiting by Size" , nameof(configuration.Member));
        }
        return true;
    }
}
