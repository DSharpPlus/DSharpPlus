using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.VoiceEntities
{
    internal sealed class VoiceUserLeavePayload
    {
        [JsonProperty("user_id")]
        public ulong UserId { get; set; }
    }
}
