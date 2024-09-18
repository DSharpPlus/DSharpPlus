namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload to <see cref="VoiceGatewayOpcode.MlsCommitWelcome"/>.
/// </summary>
internal sealed record MlsCommitWelcomePayload : IMlsPayload
{
    /// <inheritdoc/>
    public required byte[] MlsMessage { get; init; }
}
