namespace DSharpPlus.EventArgs;

using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.DmChannelDeleted"/> event.
/// </summary>
public class DmChannelDeleteEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the direct message channel that was deleted.
    /// </summary>
    public DiscordDmChannel Channel { get; internal set; }

    internal DmChannelDeleteEventArgs() : base() { }
}
