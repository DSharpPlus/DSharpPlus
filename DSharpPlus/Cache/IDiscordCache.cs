namespace DSharpPlus.Cache;

using System.Threading.Tasks;
using Entities;

public interface IDiscordCache
{
    ValueTask AddUser(DiscordUser user);
    ValueTask AddChannel(DiscordChannel channel);
    ValueTask AddGuild(DiscordGuild guild);
    ValueTask AddMessage(DiscordMessage message);
    ValueTask AddThread(DiscordThreadChannel thread);
    
    ValueTask RemoveUser(ulong userId);
    ValueTask RemoveChannel(ulong channelId);
    ValueTask RemoveGuild(ulong guildId);
    ValueTask RemoveMessage(ulong messageId);
    ValueTask RemoveThread(ulong threadId);
    
    ValueTask<bool> TryGetUser(ulong userId, out DiscordUser? user);
    ValueTask<bool> TryGetChannel(ulong channelId, out DiscordChannel? channel);
    ValueTask<bool> TryGetGuild(ulong guildId, out DiscordGuild? guild);
    ValueTask<bool> TryGetMessage(ulong messageId, out DiscordMessage? message);
    ValueTask<bool> TryGetThread(ulong threadId, out DiscordThreadChannel? thread);
}
