using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents nested data within a <see cref="VoiceSelectProtocolPayload"/>.
/// </summary>
internal sealed record VoiceSelectProtocolData
{
    /// <summary>
    /// Specifies the external IP address this connection is available under.
    /// </summary>
    [JsonPropertyName("address")]
    public required string IPAddress { get; init; }

    /// <summary>
    /// Specifies the external port this connection is available under.
    /// </summary>
    [JsonPropertyName("port")]
    public required int Port { get; init; }

    /// <summary>
    /// Specifies the encryption mode of the current connection.
    /// </summary>
    [JsonPropertyName("mode")]
    public required string EncryptionMode { get; init; }
}
