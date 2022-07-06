using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.VoiceGatewayEntities.Payloads
{
    /// <summary>
    /// The voice server should respond with an <see cref="Enums.DiscordVoiceOpCode.Ready"/> payload, which informs us of the SSRC, UDP IP/port, and supported encryption modes the voice server expects.
    /// </summary>
    public sealed record DiscordVoiceReadyPayload
    {
        [JsonPropertyName("ssrc")]
        public uint SSRC { get; init; }

        [JsonPropertyName("address")]
        public string Address { get; init; } = null!;

        [JsonPropertyName("port")]
        public ushort Port { get; init; }

        [JsonPropertyName("modes")]
        public IReadOnlyList<string> Modes { get; init; } = Array.Empty<string>();

        [Obsolete("HeartbeatInterval here is an erroneous field and should be ignored. The correct heartbeat_interval value comes from the Hello payload.")]
        [JsonPropertyName("heartbeat_interval")]
        public int HeartbeatInterval { get; init; }
    }
}
