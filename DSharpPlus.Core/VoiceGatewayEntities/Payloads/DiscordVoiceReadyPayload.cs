using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Core.VoiceGatewayEntities.Payloads
{
    /// <summary>
    /// The voice server should respond with an <see cref="Enums.DiscordVoiceOpCode.Ready"/> payload, which informs us of the SSRC, UDP IP/port, and supported encryption modes the voice server expects.
    /// </summary>
    public sealed record DiscordVoiceReadyPayload
    {
        [JsonProperty("ssrc", NullValueHandling = NullValueHandling.Ignore)]
        public uint SSRC { get; init; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; init; } = null!;

        [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
        public ushort Port { get; init; }

        [JsonProperty("modes", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> Modes { get; init; } = Array.Empty<string>();

        [Obsolete("HeartbeatInterval here is an erroneous field and should be ignored. The correct heartbeat_interval value comes from the Hello payload.")]
        [JsonProperty("heartbeat_interval", NullValueHandling = NullValueHandling.Ignore)]
        public int HeartbeatInterval { get; init; }
    }
}
