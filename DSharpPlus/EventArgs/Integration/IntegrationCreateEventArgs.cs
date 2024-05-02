
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs;
/// <summary>
/// Represents arguments for <see cref="DiscordClient.IntegrationCreated"/>
/// </summary>
public sealed class IntegrationCreateEventArgs : DiscordEventArgs
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
