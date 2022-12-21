using System.Text.Json.Serialization;
using DSharpPlus.Core.Entities;

namespace DSharpPlus.Core.Entities.VoiceGateway.Payloads
{
    /// <summary>
    /// Sent when a guild's voice server is updated. This is sent when initially connecting to voice, and when the current voice instance fails over to a new server.
    /// </summary>
    public sealed record InternalVoiceServerUpdatePayload
    {
        /// <summary>
        /// The voice connection token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; init; } = null!;

        /// <summary>
        /// The guild this voice server update is for.
        /// </summary>
        [JsonPropertyName("guild_id")]
        public InternalSnowflake GuildId { get; init; } = null!;

        /// <summary>
        /// The voice server host.
        /// </summary>
        /// <remarks>
        /// A null endpoint means that the voice server allocated has gone away and is trying to be reallocated. You should attempt to disconnect from the currently connected voice server, and not attempt to reconnect until a new voice server is allocated.
        /// </remarks>
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; init; }
    }
}
