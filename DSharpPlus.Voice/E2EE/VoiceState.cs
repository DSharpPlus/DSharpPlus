using DSharpPlus.Voice.Transport;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Cryptors;
using System.Collections.Generic;

public class VoiceState
{
    internal VoiceState() { }
    public required string SessionId { get; set; }
    public required string UserId { get; set; }
    public required string VoiceToken { get; set; }
    public required string Endpoint { get; set; }
    public required string ServerId { get; set; }
    public required string ConnectedChannel { get; set; }
    public required bool E2EEIsActive { get; set; }

    public ushort SequenceNumber => this.VoiceNegotiationTransportService.SequenceNumber;
    public uint Ssrc { get; set; }
    public List<ulong> OtherUsersInVoice { get; set; } = [];
    public DaveStateHandler? DaveStateHandler { get; set; }
    public ITransportService? VoiceNegotiationTransportService { get; set; }
    public ICryptor? VoiceCryptor { get; set; }
}

