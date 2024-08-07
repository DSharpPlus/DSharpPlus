using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;

/// <summary>
/// Represents arguments for IntegrationCreated
/// </summary>
public sealed class IntegrationCreatedEventArgs : DiscordEventArgs
{
    /// <summary>
    /// Gets the integration.
    /// </summary>
    public DiscordIntegration Integration { get; internal set; }

    /// <summary>
    /// Gets the guild the integration was added to.
    /// </summary>
    public DiscordGuild Guild { get; internal set; }
}
