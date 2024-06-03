using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.GuildCreated"/> event.
/// </summary>
public class GuildCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the guild that was created.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }

    internal GuildCreatedEventArgs() : base() { }
}
