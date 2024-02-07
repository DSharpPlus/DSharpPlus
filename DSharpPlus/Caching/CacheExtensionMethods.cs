namespace DSharpPlus.Caching;

using System.Threading.Tasks;
using Entities;

internal static class CacheExtensionMethods
{
    /// <summary>
    /// Adds a new entity to the cache, if it is not already present.
    /// </summary>
    /// <param name="cache">Cache which should be used</param>
    /// <param name="entity">Entity which will be added</param>
    /// <param name="key">Key which is used to see if its present and which will be the key for the new entity</param>
    /// <typeparam name="T">Type of <paramref name="entity"/></typeparam>
    internal static async ValueTask AddIfNotPresentAsync<T>(this IDiscordCache cache, T entity, ICacheKey key)
    {
        T? cachedEntity = await cache.TryGet<T>(key);
        if (cachedEntity is null)
        {
            await cache.Set(entity, key);
        }
    }

    /// <summary>
    /// Adds a new user to cache and overwrites the old one if it is already present.
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="newUser"></param>
    internal static async ValueTask AddUserAsync(this IDiscordCache cache, DiscordUser newUser) =>
        await cache.Set(newUser, newUser.GetCacheKey());
    
    internal static async ValueTask AddUserPresenceAsync(this IDiscordCache cache, DiscordPresence newPresence) =>
        await cache.Set(newPresence, newPresence.GetCacheKey());

    internal static async ValueTask AddGuildAsync(this IDiscordCache cache, DiscordGuild newGuild) =>
        await cache.Set(newGuild, newGuild.GetCacheKey());

    internal static async ValueTask AddChannelAsync(this IDiscordCache cache, DiscordChannel newChannel) =>
        await cache.Set(newChannel, newChannel.GetCacheKey());

    internal static async ValueTask AddMemberAsync(this IDiscordCache cache, DiscordMember newMember) =>
        await cache.Set(newMember, newMember.GetCacheKey());

    internal static async ValueTask AddMessageAsync(this IDiscordCache cache, DiscordMessage newMessage) =>
        await cache.Set(newMessage, newMessage.GetCacheKey());

    internal static async ValueTask<DiscordUser?> TryGetUserAsync(this IDiscordCache cache, ulong userId) =>
        await cache.TryGet<DiscordUser>(ICacheKey.ForUser(userId));
    
    internal static async ValueTask<DiscordPresence?> TryGetUserPresenceAsync(this IDiscordCache cache, ulong userId) =>
        await cache.TryGet<DiscordPresence>(ICacheKey.ForUser(userId));

    internal static async ValueTask<DiscordGuild?> TryGetGuildAsync(this IDiscordCache cache, ulong guildId) =>
        await cache.TryGet<DiscordGuild>(ICacheKey.ForGuild(guildId));

    internal static async ValueTask<DiscordChannel?> TryGetChannelAsync(this IDiscordCache cache, ulong channelId) =>
        await cache.TryGet<DiscordChannel>(ICacheKey.ForChannel(channelId));

    internal static async ValueTask<DiscordMember?> TryGetMemberAsync(this IDiscordCache cache, ulong memberId,
    ulong guildId) => await cache.TryGet<DiscordMember>(ICacheKey.ForMember(memberId, guildId));

    internal static async ValueTask<DiscordMessage?> TryGetMessageAsync(this IDiscordCache cache, ulong messageId) =>
        await cache.TryGet<DiscordMessage>(ICacheKey.ForMessage(messageId));
    
    internal static async ValueTask RemoveUserAsync(this IDiscordCache cache, ulong userId) =>
        await cache.Remove(ICacheKey.ForUser(userId));
    
    internal static async ValueTask RemoveUserPresenceAsync(this IDiscordCache cache, ulong userId) =>
        await cache.Remove(ICacheKey.ForUser(userId));
    
    internal static async ValueTask RemoveGuildAsync(this IDiscordCache cache, ulong guildId) =>
        await cache.Remove(ICacheKey.ForGuild(guildId));
    
    internal static async ValueTask RemoveChannelAsync(this IDiscordCache cache, ulong channelId) =>
        await cache.Remove(ICacheKey.ForChannel(channelId));
    
    internal static async ValueTask RemoveMemberAsync(this IDiscordCache cache, ulong memberId, ulong guildId) =>
        await cache.Remove(ICacheKey.ForMember(memberId, guildId));
    
    internal static async ValueTask RemoveMessageAsync(this IDiscordCache cache, ulong messageId) =>
        await cache.Remove(ICacheKey.ForMessage(messageId));
    
}
