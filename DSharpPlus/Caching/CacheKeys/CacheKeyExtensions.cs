using DSharpPlus.Entities;

namespace DSharpPlus.Caching;

public static class CacheKeyExtensions
{
    public static ICacheKey GetCacheKey(this DiscordGuild guild) => new GuildCacheKey(guild.Id);

    public static ICacheKey GetCacheKey(this DiscordChannel channel) => new ChannelCacheKey(channel.Id);

    public static ICacheKey GetCacheKey(this DiscordMessage message) => new MessageCacheKey(message.Id);

    public static ICacheKey GetCacheKey(this DiscordMember member) => new MemberCacheKey(member.Id, member._guild_id);

    public static ICacheKey GetCacheKey(this DiscordUser user) => new UserCacheKey(user.Id);
}
