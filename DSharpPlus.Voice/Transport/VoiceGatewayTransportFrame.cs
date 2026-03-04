using DSharpPlus.Voice.Protocol.Gateway;

namespace DSharpPlus.Voice.Transport;

/// <summary>
/// Represents a single received payload from the voice gateway.
/// </summary>
public readonly record struct VoiceGatewayTransportFrame
{
    /// <summary>
    /// The opcode of this payload.
    /// </summary>
    public required VoiceGatewayOpcode Opcode { get; init; }

    /// <summary>
    /// Indicates whether this is a binary (DAVE) payload or a text payload.
    /// </summary>
    public required bool IsBinary { get; init; }

    /// <summary>
    /// The payload data.
    /// </summary>
    public required byte[] Payload { get; init; }
}