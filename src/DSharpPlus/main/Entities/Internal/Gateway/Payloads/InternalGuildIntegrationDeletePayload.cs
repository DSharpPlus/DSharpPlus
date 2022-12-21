using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

/// <summary>
/// Sent when an integration is deleted.
/// </summary>
public sealed record InternalGuildIntegrationDeletePayload
{
    /// <summary>
    /// The integration id.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; init; } = null!;

    /// <summary>
    /// The id of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;

    /// <summary>
    /// The id of the bot/OAuth2 application for this discord integration.
    /// </summary>
    [JsonPropertyName("application_id")]
    public Optional<Snowflake> ApplicationId { get; init; }
}
