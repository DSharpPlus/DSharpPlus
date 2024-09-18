using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.ExecuteTransition"/>
/// </summary>
internal sealed record DaveExecuteTransitionPayload : IVoicePayload
{
    /// <summary>
    /// The previously established identifier for the transition to execute.
    /// </summary>
    [JsonPropertyName("transition_id")]
    public required uint TransitionId { get; init; }
}
