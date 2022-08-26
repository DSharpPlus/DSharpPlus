using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    [DiscordGatewayPayload("GUILD_INTEGRATIONS_UPDATE")]
    public sealed record DiscordGuildIntegrationsUpdatePayload
    {
        /// <summary>
        /// The id of the guild whose integrations were updated.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;
    }
}
