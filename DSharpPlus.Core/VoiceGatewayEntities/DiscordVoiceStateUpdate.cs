using System;
using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.VoiceGatewayEntities
{
    /// <summary>
    /// To inform the gateway of our intent to establish voice connectivity, we first send an <see cref="Enums.DiscordGatewayOpCode.VoiceStateUpdate"/> payload.
    /// </summary>
    [DiscordGatewayPayload("VOICE_STATE_UPDATE")]
    public sealed record DiscordVoiceStateUpdate
    {
        /// <summary>
        /// The guild id this voice state is for.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The channel id this user is connected to.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake? ChannelId { get; init; }

        /// <summary>
        /// The user id this voice state is for.
        /// </summary>
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The guild member this voice state is for.
        /// </summary>
        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordGuildMember> Member { get; init; }

        /// <summary>
        /// The session id for this voice state.
        /// </summary>
        [JsonProperty("session_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; init; } = null!;

        /// <summary>
        /// Whether this user is deafened by the server
        /// </summary>
        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool Deaf { get; init; }

        /// <summary>
        /// Whether this user is muted by the server.
        /// </summary>
        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool Mute { get; init; }

        /// <summary>
        /// Whether this user is locally deafened.
        /// </summary>
        [JsonProperty("self_deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfDeaf { get; init; }

        /// <summary>
        /// Whether this user is locally muted.
        /// </summary>
        [JsonProperty("self_mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfMute { get; init; }

        /// <summary>
        /// Whether this user is streaming using "Go Live".
        /// </summary>
        [JsonProperty("self_stream", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<bool> SelfStream { get; init; }

        /// <summary>
        /// Whether this user's camera is enabled.
        /// </summary>
        [JsonProperty("self_video", NullValueHandling = NullValueHandling.Ignore)]
        public bool SelfVideo { get; init; }

        /// <summary>
        /// Whether this user is muted by the current user.
        /// </summary>
        [JsonProperty("suppress", NullValueHandling = NullValueHandling.Ignore)]
        public bool Supress { get; init; }

        /// <summary>
        /// The time at which the user requested to speak.
        /// </summary>
        [JsonProperty("request_to_speak_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? RequestToSpeakTimestamp { get; init; }
    }
}
