using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Entities;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when an integration is deleted.
    /// </summary>
    [InternalGatewayPayload("INTEGRATION_DELETE")]
    public sealed record InternalGuildIntegrationDeletePayload
    {
        /// <summary>
        /// The integration id.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the bot/OAuth2 application for this discord integration.
        /// </summary>
        [JsonPropertyName("application_id")]
        public Optional<InternalSnowflake> ApplicationId { get; init; }
    }
}
