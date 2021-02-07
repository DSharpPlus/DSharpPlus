using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public class DiscordGuildWelcomeScreenChannel
    {
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        [JsonProperty("emoji_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? EmojiId { get; internal set; }

        [JsonProperty("emoji_name", NullValueHandling = NullValueHandling.Ignore)]
        public string EmojiName { get; internal set; }
    }
}
