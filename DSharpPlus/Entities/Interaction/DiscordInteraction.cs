using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an interaction that was invoked.
    /// </summary>
    public sealed class DiscordInteraction : SnowflakeObject
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

        /// <summary>
        /// Gets the Id of the guild that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public ulong? GuildId { get; internal set; }

        /// <summary>
        /// Gets the guild that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => (this.Discord as DiscordClient).InternalGetCachedGuild(this.GuildId);

        /// <summary>
        /// Gets the Id of the channel that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public ulong ChannelId { get; internal set; }

        /// <summary>
        /// Gets the channel that invoked this interaction.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel
            => (this.Discord as DiscordClient).InternalGetCachedChannel(this.ChannelId);

        /// <summary>
        /// Gets the user that invoked this interaction.
        /// <para>This can be cast to a <see cref="DiscordMember"/> if created in a guild.</para>
        /// </summary>
        [JsonIgnore]
        public DiscordUser User { get; internal set; }

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

        /// <summary>
        /// Gets the ID of the application that created this interaction.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong ApplicationId { get; internal set; }
    }
}
