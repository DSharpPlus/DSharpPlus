namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Clientbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.MlsExternalSender"/>.
/// </summary>
internal sealed record MlsExternalSenderPayload : IMlsPayload
{
    /// <inheritdoc/>
    public required byte[] MlsMessage { get; init; }
}
