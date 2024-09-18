using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.TransitionReady"/>
/// </summary>
internal sealed record DaveTransitionReadyPayload : IVoicePayload
{
    /// <summary>
    /// The previously announced transition ID the application is ready to execute.
    /// </summary>
    [JsonPropertyName("transition_id")]
    public required uint TransitionId { get; init; }
}
