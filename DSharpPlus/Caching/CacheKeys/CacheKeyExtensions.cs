using DSharpPlus.Entities;

namespace DSharpPlus.Caching;

public static class CacheKeyExtensions
{
    public static GuildCacheKey GetCacheKey(this DiscordGuild guild) => new(guild.Id);

    public static ChannelCacheKey GetCacheKey(this DiscordChannel channel) => new(channel.Id);

    public static MessageCacheKey GetCacheKey(this DiscordMessage message) => new(message.Id);

    public static MemberCacheKey GetCacheKey(this DiscordMember member) => new(member.Id, member._guild_id);

    public static UserCacheKey GetCacheKey(this DiscordUser user) => new(user.Id);
}
