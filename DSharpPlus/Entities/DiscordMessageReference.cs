using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents data from the original message.
    /// </summary>
    public class DiscordMessageReference
    {
        /// <summary>
        /// Gets the message ID of the original message.
        /// </summary>
        [JsonProperty("message_id")]
        public ulong? MessageId { get; internal set; } 

        /// <summary>
        /// Gets the channel ID of the original message.
        /// </summary>
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the guild ID of the original message.
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong? GuildId { get; internal set; }

        internal DiscordMessageReference() { }
    }
}
