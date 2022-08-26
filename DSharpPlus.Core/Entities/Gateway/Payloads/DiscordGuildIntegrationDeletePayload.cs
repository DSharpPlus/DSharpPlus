using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.Gateway.Payloads
{
    /// <summary>
    /// Sent when an integration is deleted.
    /// </summary>
    [DiscordGatewayPayload("INTEGRATION_DELETE")]
    public sealed record DiscordGuildIntegrationDeletePayload
    {
        /// <summary>
        /// The integration id.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the bot/OAuth2 application for this discord integration.
        /// </summary>
        [JsonPropertyName("application_id")]
        public Optional<DiscordSnowflake> ApplicationId { get; init; }
    }
}
