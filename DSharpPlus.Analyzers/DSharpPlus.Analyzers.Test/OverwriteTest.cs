namespace DSharpPlus.Analyzers.Test;

using DSharpPlus.Entities;

public class OverwriteTest
{
    public async Task AddOverwritesAsync(DiscordChannel channel, DiscordMember member, DiscordMember member2)
    {
        await channel.AddOverwriteAsync(member, Permissions.BanMembers);
        await channel.AddOverwriteAsync(member2, Permissions.KickMembers);
    }
}
