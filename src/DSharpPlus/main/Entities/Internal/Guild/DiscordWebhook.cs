using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Used to represent a webhook.
    /// </summary>
    public sealed record DiscordWebhook
    {
        /// <summary>
        /// The id of the webhook.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The <see cref="DiscordWebhookType"/> of the webhook.
        /// </summary>
        [JsonPropertyName("type")]
        public DiscordWebhookType Type { get; init; }

        /// <summary>
        /// The guild id this webhook is for, if any.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake?> GuildId { get; init; }

        /// <summary>
        /// The channel id this webhook is for, if any.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake? ChannelId { get; init; }

        /// <summary>
        /// The user this webhook was created by (not returned when getting a webhook with its token).
        /// </summary>
        [JsonPropertyName("user")]
        public Optional<DiscordUser> User { get; init; }

        /// <summary>
        /// The default name of the webhook.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>
        /// The default user avatar hash of the webhook.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string? Avatar { get; init; }

        /// <summary>
        /// The secure token of the webhook (returned for Incoming Webhooks).
        /// </summary>
        [JsonPropertyName("token")]
        public Optional<string> Token { get; init; }

        /// <summary>
        /// The bot/OAuth2 application that created this webhook.
        /// </summary>
        [JsonPropertyName("application_id")]
        public DiscordSnowflake? ApplicationId { get; init; }

        /// <summary>
        /// The guild of the channel that this webhook is following (returned for Channel Follower Webhooks).
        /// </summary>
        [JsonPropertyName("source_guild")]
        public Optional<DiscordGuild> SourceGuild { get; init; }

        /// <summary>
        /// The channel that this webhook is following (returned for Channel Follower Webhooks).
        /// </summary>
        [JsonPropertyName("source_channel")]
        public Optional<DiscordChannel> SourceChannel { get; init; }

        /// <summary>
        /// The url used for executing the webhook (returned by the webhooks OAuth2 flow).
        /// </summary>
        [JsonPropertyName("url")]
        public Optional<string> Url { get; init; }
    }
}
