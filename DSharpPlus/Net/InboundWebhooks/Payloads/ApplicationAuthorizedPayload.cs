using System.Collections.Generic;

using DSharpPlus.Entities;

using Newtonsoft.Json;

namespace DSharpPlus.Net.InboundWebhooks.Payloads;

/// <summary>
/// Payload for <see cref="DiscordWebhookEventBodyType.ApplicationAuthorized"/>.
/// </summary>
internal sealed class ApplicationAuthorizedPayload
{
    /// <summary>
    /// The context this authorization occurred in.
    /// </summary>
    [JsonProperty("integration_type")]
    public DiscordApplicationIntegrationType IntegrationType { get; set; }

    /// <summary>
    /// The user who authorized the application.
    /// </summary>
    [JsonProperty("user")]
    public DiscordUser User { get; set; }

    /// <summary>
    /// A list of scopes the user authorized to.
    /// </summary>
    [JsonProperty("scopes")]
    public IReadOnlyList<string> Scopes { get; set; }

    /// <summary>
    /// The guild the application was authorized for. Only applicable if <see cref="IntegrationType"/> is
    /// <see cref="DiscordApplicationIntegrationType.GuildInstall"/>.
    /// </summary>
    [JsonProperty("guild")]
    public DiscordGuild? Guild { get; set; }
}
