using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.Entities.VoiceGateway.Commands
{
    /// <summary>
    /// To notify clients that you are speaking or have stopped speaking, send an Opcode 5 Speaking payload:
    /// </summary>
    /// <remarks>
    /// You must send at least one Opcode 5 Speaking payload before sending voice data, or you will be disconnected with an invalid SSRC error.
    /// </remarks>
    public sealed record DiscordVoiceSpeakingCommand
    {
        public DiscordVoiceSpeakingIndicators Speaking { get; init; }
        public int Delay { get; init; }
        public uint SSRC { get; init; }
    }
}
