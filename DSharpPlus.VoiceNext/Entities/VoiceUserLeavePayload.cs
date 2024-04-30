namespace DSharpPlus.VoiceNext.Entities;
using Newtonsoft.Json;

internal sealed class VoiceUserLeavePayload
{
    [JsonProperty("user_id")]
    public ulong UserId { get; set; }
}
