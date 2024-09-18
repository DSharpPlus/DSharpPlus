using System.Text.Json.Serialization;

namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload to <see cref="VoiceGatewayOpcode.MlsInvalidCommitWelcome"/>.
/// </summary>
internal sealed record MlsInvalidCommitWelcomePayload
{
    /// <summary>
    /// Specifies the ID of the unprocessable transition, asking the gateway to remove and readd the member.
    /// </summary>
    [JsonPropertyName("transition_id")]
    public required int TransitionId { get; init; }
}
