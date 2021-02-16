using System;
using Newtonsoft.Json;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an interaction that was invoked.
    /// </summary>
    public class DiscordInteraction : SnowflakeObject
    {
        /// <summary>
        /// Gets the type of interaction invoked. 
        /// </summary>
        [JsonProperty("type")]
        public InteractionType Type { get; internal set; }

        /// <summary>
        /// Gets the command data for this interaction.
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordInteractionData Data { get; internal set; }

        [JsonIgnore]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the guild that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

        [JsonIgnore]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the channel that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel
            => (this.Discord as DiscordClient).InternalGetCachedChannel(this.ChannelId);

        /// <summary>
        /// Gets the member that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordMember Member { get; internal set; }

        /// <summary>
        /// Gets the continuation token for responding to this interaction.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; internal set; }

        /// <summary>
        /// Gets the version number for this interaction type.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; internal set; }
    }
}
