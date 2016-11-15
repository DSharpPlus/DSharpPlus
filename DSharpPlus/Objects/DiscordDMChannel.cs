using Newtonsoft.Json;

namespace DSharpPlus
{
    public class DiscordDMChannel : DiscordChannel
    {
        [JsonProperty("recipient")]
        public DiscordUser Recipient { get; internal set; }
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong LastMessageID { get; internal set; }
    }
}
