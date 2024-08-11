using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for IntegrationUpdated
/// </summary>
public sealed class IntegrationUpdatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the integration.
    /// </summary>
    public DiscordIntegration Integration { get; internal set; }

    /// <summary>
    /// Gets the guild the integration was updated in.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }
}
