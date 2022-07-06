using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// A <see cref="DiscordRole"/>'s metadata.
    /// </summary>
    public sealed record DiscordRoleTags
    {
        /// <summary>
        /// The id of the bot this role belongs to.
        /// </summary>
        [JsonPropertyName("bot_id")]
        public Optional<DiscordSnowflake> BotId { get; init; }

        /// <summary>
        /// The id of the integration this role belongs to.
        /// </summary>
        [JsonPropertyName("integration_id")]
        public Optional<DiscordSnowflake> IntegrationId { get; init; }

        /// <summary>
        /// Whether this is the guild's premium subscriber role.
        /// </summary>
        /// <remarks>
        /// Null when it is the guild's premium subscriber role, otherwise <see cref="Optional{T}.Empty"/>. You should use <see cref="Optional{T}.HasValue"/> to check if this is the guild's premium subscriber role.
        /// </remarks>
        [JsonPropertyName("premium_subscriber")]
        internal Optional<bool> PremiumSubscriber { get; init; }
    }
}
