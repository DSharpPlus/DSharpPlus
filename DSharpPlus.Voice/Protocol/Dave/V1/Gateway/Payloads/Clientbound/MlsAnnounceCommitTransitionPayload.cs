namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload to <see cref="VoiceGatewayOpcode.MlsAnnounceCommitTransition"/>.
/// </summary>
internal sealed record MlsAnnounceCommitTransitionPayload : IMlsPayload
{
    /// <summary>
    /// The identifier of the commit transition as previously announced.
    /// </summary>
    public required ushort TransitionId { get; init; }

    /// <inheritdoc/>
    public required byte[] MlsMessage { get; init; }
}
