namespace DSharpPlus.Cache;

/// <summary>
/// 
/// </summary>
public interface ICacheKey
{
    public const string KeyPrefix = "dsharpplus";
    
    public static ICacheKey ForGuild(ulong id) => new GuildCacheKey(id);
    public static ICacheKey ForChannel(ulong id) => new ChannelCacheKey(id);
    public static ICacheKey ForMessage(ulong id) => new MessageCacheKey(id);
    public static ICacheKey ForMember(ulong id, ulong guildId) => new MemberCacheKey(id, guildId);
    public static ICacheKey ForUser(ulong id) => new UserCacheKey(id);
}
