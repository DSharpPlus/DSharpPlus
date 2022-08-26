using System;
using DSharpPlus.Core.Attributes;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.VoiceGateway
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
        [JsonPropertyName("guild_id")]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// The channel id this user is connected to.
        /// </summary>
        [JsonPropertyName("channel_id")]
        public DiscordSnowflake? ChannelId { get; init; }

        /// <summary>
        /// The user id this voice state is for.
        /// </summary>
        [JsonPropertyName("user_id")]
        public DiscordSnowflake UserId { get; init; } = null!;

        /// <summary>
        /// The guild member this voice state is for.
        /// </summary>
        [JsonPropertyName("member")]
        public Optional<DiscordGuildMember> Member { get; init; }

        /// <summary>
        /// The session id for this voice state.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; init; } = null!;

        /// <summary>
        /// Whether this user is deafened by the server
        /// </summary>
        [JsonPropertyName("deaf")]
        public bool Deaf { get; init; }

        /// <summary>
        /// Whether this user is muted by the server.
        /// </summary>
        [JsonPropertyName("mute")]
        public bool Mute { get; init; }

        /// <summary>
        /// Whether this user is locally deafened.
        /// </summary>
        [JsonPropertyName("self_deaf")]
        public bool SelfDeaf { get; init; }

        /// <summary>
        /// Whether this user is locally muted.
        /// </summary>
        [JsonPropertyName("self_mute")]
        public bool SelfMute { get; init; }

        /// <summary>
        /// Whether this user is streaming using "Go Live".
        /// </summary>
        [JsonPropertyName("self_stream")]
        public Optional<bool> SelfStream { get; init; }

        /// <summary>
        /// Whether this user's camera is enabled.
        /// </summary>
        [JsonPropertyName("self_video")]
        public bool SelfVideo { get; init; }

        /// <summary>
        /// Whether this user is muted by the current user.
        /// </summary>
        [JsonPropertyName("suppress")]
        public bool Supress { get; init; }

        /// <summary>
        /// The time at which the user requested to speak.
        /// </summary>
        [JsonPropertyName("request_to_speak_timestamp")]
        public DateTimeOffset? RequestToSpeakTimestamp { get; init; }
    }
}
