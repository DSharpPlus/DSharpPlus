using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Lavalink;

public static class DiscordClientExtensions
{
    /// <summary>
    /// Connects to this voice channel using Lavalink.
    /// </summary>
    /// <param name="channel">Channel to connect to.</param>
    /// <param name="node">Lavalink node to connect through.</param>
    /// <returns>If successful, the Lavalink client.</returns>
    [Obsolete("DSharpPlus.Lavalink is deprecated for removal.", true)]
    public static Task ConnectAsync(this DiscordChannel channel, LavalinkNodeConnection node)
    {
        if (channel == null)
        {
            throw new NullReferenceException();
        }

        if (channel.Guild == null)
        {
            throw new InvalidOperationException("Lavalink can only be used with guild channels.");
        }

        if (channel.Type is not DiscordChannelType.Voice and not DiscordChannelType.Stage)
        {
            throw new InvalidOperationException("You can only connect to voice and stage channels.");
        }

        if (channel.Discord is not DiscordClient discord || discord == null)
        {
            throw new NullReferenceException();
        }

        throw new NotImplementedException("DSharpPlus.Lavalink is defunct.");
    }
}
