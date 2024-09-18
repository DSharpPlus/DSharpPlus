namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload to <see cref="VoiceGatewayOpcode.MlsProposals"/>.
/// </summary>
internal sealed record MlsProposalsPayload : IMlsPayload
{
    /// <inheritdoc/>
    public required byte[] MlsMessage { get; init; }
}
