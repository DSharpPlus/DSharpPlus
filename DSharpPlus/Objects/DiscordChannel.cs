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
        public int Position { get; set; }
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
        public async Task Delete() => await DiscordClient.InternalDeleteChannel(ID);
        public async Task<DiscordMessage> GetMessage(ulong ID) => await DiscordClient.InternalGetMessage(this.ID, ID);
        public async Task ModifyPosition(int position) => await DiscordClient.InternalModifyGuildChannelPosition(GuildID, ID, position);
        /// <summary>  
        ///  Only set ONE of the three parameters. They are Message ID's
        /// </summary> 
        public async Task<List<DiscordMessage>> GetMessages(ulong around = 0, ulong before = 0, ulong after = 0) => await DiscordClient.InternalGetChannelMessages(ID, around, before, after);
        public async Task BulkDeleteMessages(List<ulong> MessageIDs) => await DiscordClient.InternalBulkDeleteMessages(ID, MessageIDs);
        public async Task<List<DiscordInvite>> GetInvites() => await DiscordClient.InternalGetChannelInvites(ID);
        public async Task<DiscordInvite> CreateInvite(int MaxAge = 86400, int MaxUses = 0, bool temporary = false, bool unique = false)
            => await DiscordClient.InternalCreateChannelInvite(ID, MaxAge, MaxUses, temporary, unique);
        public async Task DeleteChannelPermission(ulong OverwriteID) => await DiscordClient.InternalDeleteChannelPermission(ID, OverwriteID);
        public async Task TriggerTyping() => await DiscordClient.InternalTriggerTypingIndicator(ID);
        public async Task<List<DiscordMessage>> GetPinnedMessages() => await DiscordClient.InternalGetPinnedMessages(ID);
        /// <summary>
        /// Only use for Group DM's! Whitelised bots only. Requires user's oauth2 access token
        /// </summary>
        public async Task AddDMRecipient(ulong UserID, string accesstoken) => await DiscordClient.InternalGroupDMAddRecipient(ID, UserID, accesstoken);
        /// <summary>
        /// Only use for Group DM's!
        /// </summary>
        public async Task RemoveDMRecipient(ulong UserID, string accesstoken) => await DiscordClient.InternalGroupDMRemoveRecipient(ID, UserID);
        public async Task<DiscordWebhook> CreateWebhook(string name = "", string base64avatar = "") => await DiscordClient.InternalCreateWebhook(ID, name, base64avatar);
        public async Task<List<DiscordWebhook>> GetWebhooks() => await DiscordClient.InternalGetChannelWebhooks(ID);
        #endregion

    }
}
