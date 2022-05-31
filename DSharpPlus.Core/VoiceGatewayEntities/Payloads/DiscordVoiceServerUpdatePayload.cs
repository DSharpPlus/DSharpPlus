using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.RestEntities;
using Newtonsoft.Json;

namespace DSharpPlus.Core.VoiceGatewayEntities.Payloads
{
    /// <summary>
    /// Sent when a guild's voice server is updated. This is sent when initially connecting to voice, and when the current voice instance fails over to a new server.
    /// </summary>
    [DiscordGatewayPayload("VOICE_SERVER_UPDATE")]
    public sealed record DiscordVoiceServerUpdatePayload
    {
        /// <summary>
        /// The voice connection token.
        /// </summary>
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The guild this voice server update is for.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The voice server host.
        /// </summary>
        /// <remarks>
        /// A null endpoint means that the voice server allocated has gone away and is trying to be reallocated. You should attempt to disconnect from the currently connected voice server, and not attempt to reconnect until a new voice server is allocated.
        /// </remarks>
        [JsonProperty("endpoint", NullValueHandling = NullValueHandling.Ignore)]
        public string? Endpoint { get; init; }
    }
}
