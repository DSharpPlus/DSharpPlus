namespace DSharpPlus.EventArgs;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildCreated"/> event.
/// </summary>
public class GuildCreateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that was created.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal GuildCreateEventArgs() : base() { }
}
