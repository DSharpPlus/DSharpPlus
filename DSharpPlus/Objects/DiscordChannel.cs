using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;

namespace DSharpPlus
{
    public class DiscordChannel : SnowflakeObject
    {
        [JsonProperty("guild_id")]
        public ulong GuildID { get; internal set; }
        public DiscordGuild Parent => DiscordClient._guilds[GuildID];
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("type")]
        public ChannelType Type { get; internal set; }
        [JsonProperty("position")]
        public int Position { get; internal set; }
        [JsonProperty("is_private")]
        public bool IsPrivate { get; internal set; }
        [JsonProperty("permission_overwrites")]
        public List<DiscordOverwrite> PermissionOverwrites { get; internal set; }

        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; internal set; } = "";
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong LastMessageID { get; internal set; } = 0;

        [JsonProperty("bitrate")]
        public int Bitrate { get; internal set; }
        [JsonProperty("user_limit")]
        public int UserLimit { get; internal set; }

        #region Channel Modification
        public async Task DeleteChannel() => await DiscordClient.InternalDeleteChannel(ID);
        #endregion

        #region Messages
        public async Task<DiscordMessage> SendMessage(string content, bool tts = false) => await DiscordClient.InternalCreateMessage(ID, content, tts);
        public async Task<DiscordMessage> SendFile(string filepath, string filename, string content = "", bool tts = false) => await DiscordClient.InternalUploadFile(ID, filepath, filename, content, tts);
        #endregion

    }
}
