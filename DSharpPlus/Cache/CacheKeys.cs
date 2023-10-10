namespace DSharpPlus.Cache;

using Entities;

public interface ICacheKey
{
    public const string KeyPrefix = "dsharpplus";
}

public readonly record struct GuildCacheKey : ICacheKey
{
    //key format is "{keyPrefix}-guild-{guildId}"
    public ulong Id { get; init; }
    public GuildCacheKey(ulong id) => this.Id = id;

    public override string ToString() => $"{ICacheKey.KeyPrefix}-guild-{this.Id}";
}

public readonly record struct ChannelCacheKey : ICacheKey
{
    //key format is "{keyPrefix}-channel-{channelId}"
    public ulong Id { get; init; }
    public ChannelCacheKey(ulong id) => this.Id = id;
    
    public override string ToString() => $"{ICacheKey.KeyPrefix}-channel-{this.Id}";
}

public readonly record struct MessageCacheKey : ICacheKey
{
    //key format is "{keyPrefix}-message-{messageId}"
    public ulong Id { get; init; }
    public MessageCacheKey(ulong id) => this.Id = id;
    
    public override string ToString() => $"{ICacheKey.KeyPrefix}-message-{this.Id}";
}

public readonly record struct UserCacheKey : ICacheKey
{
    //key format is "{keyPrefix}-user-{userId}"
    public ulong Id { get; init; }
    public UserCacheKey(ulong id) => this.Id = id;
    
    public override string ToString() => $"user-{this.Id}";
}

public readonly record struct MemberCacheKey : ICacheKey
{
    //key format is "{keyPrefix}-guild-{this.GuildId}-member-{this.Id}"
    public ulong Id { get; init; }
    public ulong GuildId { get; init; }

    public MemberCacheKey(ulong id, ulong guildId)
    {
        this.Id = id;
        this.GuildId = guildId;
    }

    public override string ToString() => $"{ICacheKey.KeyPrefix}-guild-{this.GuildId}-member-{this.Id}";
}
