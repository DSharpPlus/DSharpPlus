using DSharpPlus.Voice.Transport;
using DSharpPlus.Voice.E2EE;
using DSharpPlus.Voice.Cryptors;
using System.Collections.Generic;

public class VoiceState
{
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public string? VoiceToken { get; set; }
    public string? Endpoint { get; set; }
    public uint Ssrc { get; set; }
    public List<ulong> OtherUsersInVoice { get; set; } = [];
    public DaveStateHandler? DaveStateHandler { get; set; }
    public ITransportService? VoiceNegotiationTransportService { get; set; }
    public ICryptor? VoiceCryptor { get; set; }
    public string? ServerId { get; set; }
    public string? ConnectedChannel { get; set; }
    public bool E2EEIsActive { get; set; }
}

