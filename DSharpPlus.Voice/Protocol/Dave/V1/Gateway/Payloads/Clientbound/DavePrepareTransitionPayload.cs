using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.PrepareTransition"/>.
/// </summary>
internal sealed record DavePrepareTransitionPayload : IVoicePayload
{
    /// <summary>
    /// The protocol version after the transition. DSharpPlus does not support downgrading to version 0.
    /// </summary>
    [JsonPropertyName("protocol_version")]
    public required uint ProtocolVersion { get; init; }

    /// <summary>
    /// An identifier for the currently ongoing transition for future handling. If this is zero, the transition
    /// is meant for re/initialization and can be executed immediately without waiting for opcode 22 execute transition.
    /// </summary>
    [JsonPropertyName("transition_id")]
    public required uint TransitionId { get; init; }
}
