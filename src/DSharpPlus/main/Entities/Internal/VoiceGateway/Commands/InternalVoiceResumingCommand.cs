namespace DSharpPlus.Entities.Internal.VoiceGateway.Commands;

/// <summary>
/// When your client detects that its connection has been severed, it should open a new WebSocket connection. Once the new connection has been opened, your client should send an <see cref="Enums.InternalVoiceOpCode.Resume"/> payload.
/// </summary>
public sealed record InternalVoiceResumingCommand
{
    public InternalSnowflake ServerId { get; init; } = null!;
    public string SessionId { get; init; } = null!;
    public string Token { get; init; } = null!;
}
