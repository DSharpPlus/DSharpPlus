using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for application command events.
/// </summary>
public sealed class ApplicationCommandEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the command that was modified.
    /// </summary>
    public DiscordApplicationCommand Command { get; internal set; }

    /// <summary>
    /// Gets the optional guild of the command.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }
}
