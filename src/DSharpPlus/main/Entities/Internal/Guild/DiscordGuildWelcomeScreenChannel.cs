using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordGuildWelcomeScreenChannel
    {
        /// <summary>
        /// The channel's id.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The description shown for the channel.
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; init; } = null!;

        /// <summary>
        /// The emoji id, if the emoji is custom.
        /// </summary>
        [JsonPropertyName("emoji_id")]
        public DiscordSnowflake? EmojiId { get; init; }

        /// <summary>
        /// The emoji name if custom, the unicode character if standard, or null if no emoji is set.
        /// </summary>
        [JsonPropertyName("emoji_name")]
        public string? EmojiName { get; init; } = null!;
    }
}
