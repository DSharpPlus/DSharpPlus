namespace DSharpPlus.Cache;

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
    
    public ValueTask AddUser(DiscordUser user)
    { 
        this._cache.Set(user.Id, user, this._userOptions);
        return default;
    }

    public ValueTask AddChannel(DiscordChannel channel)
    {
        this._cache.Set(channel.Id, channel, this._channelOptions);
        return default;
    }

    public ValueTask AddGuild(DiscordGuild guild)
    {
        this._cache.Set(guild.Id, guild, this._guildOptions);
        return default;
    }

    public ValueTask AddMessage(DiscordMessage message)
    {
        this._cache.Set(message.Id, message, this._messageOptions);
        return default;
    }
    
    public ValueTask AddThread(DiscordThreadChannel thread)
    {
        this._cache.Set(thread.Id, thread, this._threadOptions);
        return default;
    }

    public ValueTask RemoveUser(ulong userId)
    {
        this._cache.Remove(userId);
        return default;
    }

    public ValueTask RemoveChannel(ulong channelId)
    {
        this._cache.Remove(channelId);
        return default;
    }

    public ValueTask RemoveGuild(ulong guildId)
    {
        this._cache.Remove(guildId);
        return default;
    }

    public ValueTask RemoveMessage(ulong messageId)
    {
        this._cache.Remove(messageId);
        return default;
    }
    
    public ValueTask RemoveThread(ulong threadId)
    {
        this._cache.Remove(threadId);
        return default;
    }

    public ValueTask<bool> TryGetUser(ulong userId, out DiscordUser? user)
    {
        user = this._cache.Get<DiscordUser>(userId);
        return new ValueTask<bool>(user is not null);
    }

    public ValueTask<bool> TryGetChannel(ulong channelId, out DiscordChannel? channel)
    {
        channel = this._cache.Get<DiscordChannel>(channelId);
        return new ValueTask<bool>(channel is not null);
    }

    public ValueTask<bool> TryGetGuild(ulong guildId, out DiscordGuild? guild)
    {
        guild = this._cache.Get<DiscordGuild>(guildId);
        return new ValueTask<bool>(guild is not null);
    }

    public ValueTask<bool> TryGetMessage(ulong messageId, out DiscordMessage? message)
    {
        message = this._cache.Get<DiscordMessage>(messageId);
        return new ValueTask<bool>(message is not null);
    }
    
    public ValueTask<bool> TryGetThread(ulong threadId, out DiscordThreadChannel? thread)
    {
        thread = this._cache.Get<DiscordThreadChannel>(threadId);
        return new ValueTask<bool>(thread is not null);
    }
}
