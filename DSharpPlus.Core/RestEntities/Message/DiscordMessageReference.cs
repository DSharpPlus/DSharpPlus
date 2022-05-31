using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// See https://discord.com/developers/docs/resources/channel#message-reference-object-message-reference-structure
    /// </summary>
    public sealed record DiscordMessageReference
    {
        /// <summary>
        /// The id of the originating message.
        /// </summary>
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> MessageId { get; init; }

        /// <summary>
        /// The id of the originating message's channel.
        /// </summary>
        /// <remarks>
        /// channel_id is optional when creating a reply, but will always be present when receiving an event/response that includes this data model.
        /// </remarks>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> ChannelId { get; init; }

        /// <summary>
        /// The id of the originating message's guild.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// When sending, whether to error if the referenced message doesn't exist instead of sending as a normal (non-reply) message, default true.
        /// </summary>
        [JsonProperty("fail_if_not_exists", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> FailIfNotExists { get; init; }
    }
}
