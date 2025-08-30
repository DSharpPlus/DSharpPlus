using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Transport.Models.VoicePayloads.Inbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.PrepareEpoch"/>.
/// </summary>
internal sealed record DavePrepareEpochPayload
{
    /// <summary>
    /// Specifies the protocol version used in an upcoming DAVE epoch.
    /// </summary>
    [JsonPropertyName("protocol_version")]
    public required uint ProtocolVersion { get; init; }

    /// <summary>
    /// The identifier of the given epoch. If this equals 1, a new MLS group is
    /// to be created for the given protocol version.
    /// </summary>
    [JsonPropertyName("epoch")]
    public required uint EpochId { get; init; }
}
