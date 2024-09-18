using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.Ready"/>.
/// </summary>
internal sealed record VoiceReadyPayload : IVoicePayload
{
    /// <summary>
    /// Provides the SSRC for the current connection.
    /// </summary>
    [JsonPropertyName("ssrc")]
    public required int SSRC { get; init; }

    /// <summary>
    /// Provides the IP address for the current connection.
    /// </summary>
    [JsonPropertyName("ip")]
    public required string IPAddress { get; init; }

    /// <summary>
    /// Provides the port for the current connection.
    /// </summary>
    [JsonPropertyName("port")]
    public required int Port { get; init; }

    /// <summary>
    /// Informs the client of supported encryption modes.
    /// </summary>
    [JsonPropertyName("modes")]
    public required IReadOnlyList<string> EncryptionModes { get; init; }

    // there is a heartbeat interval sent here, but it's incorrect, so we don't care
}
