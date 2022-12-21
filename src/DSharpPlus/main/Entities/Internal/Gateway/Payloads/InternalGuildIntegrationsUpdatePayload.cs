using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    [InternalGatewayPayload("GUILD_INTEGRATIONS_UPDATE")]
    public sealed record InternalGuildIntegrationsUpdatePayload
    {
        /// <summary>
        /// The id of the guild whose integrations were updated.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;
    }
}
