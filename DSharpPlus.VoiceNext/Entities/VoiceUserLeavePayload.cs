using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.Entities
{
    internal sealed class VoiceUserLeavePayload
    {
        [JsonProperty("user_id")]
        public ulong UserId { get; set; }
    }
}
