using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.SessionDescription"/>.
/// </summary>
internal sealed record VoiceSessionDescriptionPayload : IVoicePayload
{
    /// <summary>
    /// Specifies the selected session encryption mode.
    /// </summary>
    [JsonPropertyName("mode")]
    public required string EncryptionMode { get; init; }

    /// <summary>
    /// Specifies the secret key used for transport encryption and sending voice data.
    /// </summary>
    [JsonPropertyName("secret_key")]
    [JsonConverter(typeof(ByteArrayConverter))]
    public required byte[] SecretKey { get; init; }

    /// <summary>
    /// Specifies the selected DAVE protocol version.
    /// </summary>
    [JsonPropertyName("dave_protocol_version")]
    public required int DaveProtocolVersion { get; init; }
}
