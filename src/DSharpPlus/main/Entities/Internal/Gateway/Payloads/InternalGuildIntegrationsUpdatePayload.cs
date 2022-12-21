using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal.Gateway.Payloads;

public sealed record InternalGuildIntegrationsUpdatePayload
{
    /// <summary>
    /// The id of the guild whose integrations were updated.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Snowflake GuildId { get; init; } = null!;
}
