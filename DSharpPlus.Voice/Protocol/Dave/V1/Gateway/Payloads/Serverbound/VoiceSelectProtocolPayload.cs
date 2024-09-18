using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.SelectProtocol"/>.
/// </summary>
internal sealed record class VoiceSelectProtocolPayload : IVoicePayload
{
    /// <summary>
    /// Specifies the protocol used for this connection.
    /// </summary>
    [JsonPropertyName("protocol")]
    public required string Protocol { get; init; }

    /// <summary>
    /// Contains additional information for this connection.
    /// </summary>
    [JsonPropertyName("data")]
    public required VoiceSelectProtocolData Data { get; init; }
}
