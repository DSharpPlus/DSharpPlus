using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// Additional data used when an action is executed. Different fields are relevant based on value of <see cref="DiscordGuildAutomoderationActionType"/>.
    /// </summary>
    public sealed record DiscordAutoModerationActionMetadata
    {
        /// <summary>
        /// The channel to which user content should be logged.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake ChannelId { get; init; } = null!;

        /// <summary>
        /// The timeout duration in seconds.
        /// </summary>
        /// <remarks>
        /// Maximum of 2419200 seconds (4 weeks).
        /// </remarks>
        [JsonProperty("duration_seconds", NullValueHandling = NullValueHandling.Ignore)]
        public int DurationSeconds { get; init; }
    }
}
