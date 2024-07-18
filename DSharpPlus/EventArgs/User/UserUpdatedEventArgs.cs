using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for UserUpdated event.
/// </summary>
public class UserUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the post-update user.
    /// </summary>
    public DiscordUser UserAfter { get; internal set; }

    /// <summary>
    /// Gets the pre-update user.
    /// </summary>
    public DiscordUser UserBefore { get; internal set; }

    internal UserUpdatedEventArgs() : base() { }
}
