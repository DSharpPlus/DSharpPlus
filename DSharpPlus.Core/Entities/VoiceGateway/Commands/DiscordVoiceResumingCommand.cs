namespace DSharpPlus.Core.Entities.VoiceGateway.Commands
{
    /// <summary>
    /// When your client detects that its connection has been severed, it should open a new WebSocket connection. Once the new connection has been opened, your client should send an <see cref="Enums.DiscordVoiceOpCode.Resume"/> payload.
    /// </summary>
    public sealed record DiscordVoiceResumingCommand
    {
        public DiscordSnowflake ServerId { get; init; } = null!;
        public string SessionId { get; init; } = null!;
        public string Token { get; init; } = null!;
    }
}
