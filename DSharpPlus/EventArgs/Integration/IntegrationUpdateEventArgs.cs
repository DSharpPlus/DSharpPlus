
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;
/// <summary>
/// Represents arguments for <see cref="DiscordClient.IntegrationUpdated"/>
/// </summary>
public sealed class IntegrationUpdateEventArgs : DiscordEventArgs
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
