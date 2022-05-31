using Newtonsoft.Json;

namespace DSharpPlus.Core.VoiceGatewayEntities.Commands
{
    /// <summary>
    /// Once we've fully discovered our external IP and UDP port, we can then tell the voice WebSocket what it is, and start receiving/sending data. We do this using <see cref="Enums.DiscordVoiceOpCode.SelectProtocol"/>.
    /// </summary>
    public sealed record DiscordVoiceSelectProtocolCommand
    {
        [JsonProperty("protocol", NullValueHandling = NullValueHandling.Ignore)]
        public string Protocol { get; init; } = null!;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordVoiceSelectProtocolCommandData Data { get; init; } = null!;
    }

    public sealed record DiscordVoiceSelectProtocolCommandData
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; init; } = null!;

        [JsonProperty("port", NullValueHandling = NullValueHandling.Ignore)]
        public ushort Port { get; init; }

        /// <summary>
        /// See https://discord.com/developers/docs/topics/voice-connections#establishing-a-voice-udp-connection-encryption-modes for available options.
        /// </summary>
        [JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
        public string Mode { get; init; } = null!;
    }
}
