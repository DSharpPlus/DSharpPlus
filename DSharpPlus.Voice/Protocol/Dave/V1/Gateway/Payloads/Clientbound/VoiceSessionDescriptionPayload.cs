using System.Text.Json.Serialization;
using DSharpPlus.Voice.Protocol.Dave.V1.Serialization.Json.Converters;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

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
