using System.Collections.Generic;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace DSharpPlus.Net.InboundWebhooks;

/// <summary>
/// Contains data regarding an application being authorized to a guild or user.
/// </summary>
public sealed class DiscordApplicationAuthorizedEvent
{

    /// <summary>
    /// Gets the type of integration for the install.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordApplicationIntegrationType Type { get; internal set; }

    /// <summary>
    /// Gets the user that authorized the application.
    /// </summary>
    public DiscordUser User { get; internal set; }

    /// <summary>
    /// Gets the scopes the application was authorized for.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; internal set; }

    /// <summary>
    /// Gets the guild the application was authorized for (if applicable).
    /// </summary>
    public DiscordGuild Guild { get; internal set; }
}
