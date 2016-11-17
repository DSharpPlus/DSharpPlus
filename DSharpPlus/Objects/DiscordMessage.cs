using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordMessage : SnowflakeObject
    {
        [JsonProperty("channel_id")]
        public ulong ChannelID { get; internal set; }
        [JsonIgnore]
        public DiscordChannel Parent => DiscordClient._guilds[DiscordClient.GetGuildIdFromChannelID(ChannelID)].Channels[DiscordClient.GetChannelIndex(ChannelID)];
        [JsonProperty("author")]
        public DiscordUser Author { get; internal set; }
        [JsonProperty("content")]
        public string Content { get; internal set; }
        [JsonProperty("timestamp")]
        public string TimestampRaw { get; internal set; }
        [JsonIgnore]
        public DateTime Timestamp => DateTime.Parse(this.TimestampRaw);
        [JsonProperty("edited_timestamp")]
        public string EditedTimestampRaw { get; internal set; }
        [JsonIgnore]
        public DateTime EditedTimestamp => DateTime.Parse(this.EditedTimestampRaw);
        [JsonProperty("tts")]
        public bool TTS { get; internal set; }
        [JsonProperty("mention_everyone")]
        public bool MentionEveryone { get; internal set; }
        [JsonProperty("mentions")]
        public List<DiscordUser> Mentions { get; internal set; }
        [JsonProperty("mentioned_roles")]
        public List<DiscordRole> MentionedRoles { get; internal set; }
        [JsonProperty("attachments")]
        public List<object> Attachments { get; internal set; }
        [JsonProperty("embeds")]
        public List<DiscordEmbed> Embeds { get; internal set; }
        [JsonProperty("reactions")]
        public List<DiscordReaction> Reactions { get; internal set; }
        [JsonProperty("nonce")]
        public ulong? Nonce { get; internal set; }
        [JsonProperty("pinned")]
        public bool Pinned { get; internal set; }
        [JsonProperty("webhook_id")]
        public ulong? WebhookID { get; internal set; }

        public async Task<DiscordMessage> Edit(string content) => await DiscordClient.InternalEditMessage(ChannelID, ID, content);
        public async Task Delete() => await DiscordClient.InternalDeleteMessage(ChannelID, ID);
        public async Task Pin() => await DiscordClient.InternalAddPinnedChannelMessage(ChannelID, ID);
        public async Task Unpin() => await DiscordClient.InternalDeletePinnedChannelMessage(ChannelID, ID);
        public async Task<DiscordMessage> Respond(string content, bool tts = false) => await DiscordClient.InternalCreateMessage(ChannelID, content, tts);
        public async Task<DiscordMessage> Respond(string content, string filepath, string filename, bool tts = false) 
            => await DiscordClient.InternalUploadFile(ChannelID, filepath, filename, content, tts);
    }
}
