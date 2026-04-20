using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents the information received with a channel info event.
/// </summary>
public sealed class ChannelInfoEventArgs : DiscordEventArgs
{
    /// <summary>
    /// The guild this event pertains to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    /// <summary>
    /// The information provided for voice channels in this guild.
    /// </summary>
    public IReadOnlyList<VoiceChannelInfo> ChannelInfo { get; internal set; }
}
