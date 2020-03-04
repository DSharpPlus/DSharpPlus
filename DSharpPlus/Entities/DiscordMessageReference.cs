using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents data from the original message.
    /// </summary>
    public class DiscordMessageReference
    {
        /// <summary>
        /// Gets the original message.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel of the original message.
        /// </summary>
        public DiscordChannel Channel { get; internal set; }

        /// <summary>
        /// Gets the guild of the original message.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        public override string ToString()
            => $"Guild: {this.Guild.Id}, Channel: {this.Channel.Id}, Message: {this.Message.Id}";

        internal DiscordMessageReference() { }
    }

    internal struct InternalDiscordMessageReference
    {
        [JsonProperty("message_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? messageId { get; set; }

        [JsonProperty("channel_id")]
        internal ulong channelId { get; set; }

        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? guildId { get; set; }
    }
}
