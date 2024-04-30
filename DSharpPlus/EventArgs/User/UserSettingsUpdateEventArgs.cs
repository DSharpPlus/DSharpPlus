namespace DSharpPlus.EventArgs;
using DSharpPlus.Entities;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.UserSettingsUpdated"/> event.
/// </summary>
public class UserSettingsUpdateEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the user whose settings were updated.
    /// </summary>
    public DiscordUser User { get; internal set; }

    internal UserSettingsUpdateEventArgs() : base() { }
}
