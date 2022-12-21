using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities.VoiceGateway.Commands
{
    /// <summary>
    /// Once we've fully discovered our external IP and UDP port, we can then tell the voice WebSocket what it is, and start receiving/sending data. We do this using <see cref="Enums.InternalVoiceOpCode.SelectProtocol"/>.
    /// </summary>
    public sealed record InternalVoiceSelectProtocolCommand
    {
        [JsonPropertyName("protocol")]
        public string Protocol { get; init; } = null!;

        [JsonPropertyName("data")]
        public InternalVoiceSelectProtocolCommandData Data { get; init; } = null!;
    }

    public sealed record InternalVoiceSelectProtocolCommandData
    {
        [JsonPropertyName("address")]
        public string Address { get; init; } = null!;

        [JsonPropertyName("port")]
        public ushort Port { get; init; }

        /// <summary>
        /// See https://discord.com/developers/docs/topics/voice-connections#establishing-a-voice-udp-connection-encryption-modes for available options.
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; init; } = null!;
    }
}
