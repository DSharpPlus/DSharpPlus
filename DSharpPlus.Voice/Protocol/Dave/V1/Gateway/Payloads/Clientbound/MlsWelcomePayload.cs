namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload to <see cref="VoiceGatewayOpcode.MlsWelcome"/>.
/// </summary>
internal sealed record MlsWelcomePayload : IMlsPayload
{
    /// <summary>
    /// The transition ID for the implied group transition caused by adding the welcomed member.
    /// </summary>
    public required ushort TransitionId { get; init; }

    /// <inheritdoc/>
    public required byte[] MlsMessage { get; init; }
}
