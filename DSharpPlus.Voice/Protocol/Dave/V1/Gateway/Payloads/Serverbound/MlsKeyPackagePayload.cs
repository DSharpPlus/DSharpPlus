namespace DSharpPlus.Voice.Protocol.Dave.V1.Gateway.Payloads.Serverbound;

/// <summary>
/// Represents a payload for <see cref="VoiceGatewayOpcode.MlsKeyPackage"/>.
/// </summary>
internal sealed record MlsKeyPackagePayload : IMlsPayload
{
    /// <inheritdoc/>
    public required byte[] MlsMessage { get; init; }
}
