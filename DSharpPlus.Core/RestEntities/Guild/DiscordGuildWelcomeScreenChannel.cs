using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordGuildWelcomeScreenChannel
    {
        /// <summary>
        /// The channel's id.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The description shown for the channel.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; init; } = null!;

        /// <summary>
        /// The emoji id, if the emoji is custom.
        /// </summary>
        [JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? EmojiId { get; init; }

        /// <summary>
        /// The emoji name if custom, the unicode character if standard, or null if no emoji is set.
        /// </summary>
        [JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
        public string EmojiName { get; init; } = null!;
    }
}
