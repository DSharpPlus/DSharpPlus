using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.GatewayEntities.Payloads
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
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The id of the guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The id of the bot/OAuth2 application for this discord integration.
        /// </summary>
        [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> ApplicationId { get; init; }
    }
}
