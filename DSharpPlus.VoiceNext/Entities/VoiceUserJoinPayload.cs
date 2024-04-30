namespace DSharpPlus.VoiceNext.Entities;

using Newtonsoft.Json;

internal sealed class VoiceUserJoinPayload
{
    [JsonProperty("user_id")]
    public ulong UserId { get; private set; }

    [JsonProperty("audio_ssrc")]
    public uint SSRC { get; private set; }
}
