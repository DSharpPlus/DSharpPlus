using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DSharpPlus.Voice;
using System;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a discord channel
    /// </summary>
    public class DiscordChannel : SnowflakeObject
    {
        /// <summary>
        /// The id of the guild
        /// </summary>
        [JsonProperty("guild_id")]
        public ulong GuildID { get; internal set; }
        /// <summary>
        /// The name of the channel
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// The type of the channel
        /// </summary>
        [JsonProperty("type")]
        public ChannelType Type { get; internal set; }
        /// <summary>
        /// Sorting position of the channel
        /// </summary>
        [JsonProperty("position")]
        public int Position { get; set; }
        /// <summary>
        /// Should always be false for guild channels
        /// </summary>
        [JsonProperty("is_private")]
        public bool IsPrivate { get; internal set; }
        /// <summary>
        /// The guild
        /// </summary>
        public DiscordGuild Parent => (IsPrivate) ? new DiscordGuild() : (DiscordClient._guilds.ContainsKey(GuildID)) ? DiscordClient._guilds[GuildID] : DiscordClient.InternalGetGuild(GuildID).Result;
        /// <summary>
        /// A list of permission overwrite
        /// </summary>
        [JsonProperty("permission_overwrites")]
        public List<DiscordOverwrite> PermissionOverwrites { get; internal set; }

        /// <summary>
        /// The channel topic (Text only)
        /// </summary>
        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; internal set; } = "";
        /// <summary>
        /// The id of the last message (Text only)
        /// </summary>
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong LastMessageID { get; internal set; } = 0;

        /// <summary>
        /// The channel bitrate (Voice only)
        /// </summary>
        [JsonProperty("bitrate")]
        public int Bitrate { get; internal set; }
        /// <summary>
        /// The channel user limit (Voice only)
        /// </summary>
        [JsonProperty("user_limit")]
        public int UserLimit { get; internal set; }

        #region Functions
        /// <summary>
        /// Posts a message
        /// </summary>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendMessage(string content, bool tts = false, DiscordEmbed embed = null) => await DiscordClient.InternalCreateMessage(ID, content, tts, embed);
        /// <summary>
        /// Posts a file
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendFile(string filepath, string filename, string content = "", bool tts = false) => await DiscordClient.InternalUploadFile(ID, filepath, filename, content, tts);
        /// <summary>
        /// Deletes a guild channel
        /// </summary>
        /// <returns></returns>
        public async Task Delete() => await DiscordClient.InternalDeleteChannel(ID);
        /// <summary>
        /// Returns a specific message
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessage(ulong ID) => await DiscordClient.InternalGetMessage(this.ID, ID);
        /// <summary>
        /// Updates the channel position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task ModifyPosition(int position) => await DiscordClient.InternalModifyGuildChannelPosition(GuildID, ID, position);
        /// <summary>  
        /// Returns a list of messages.Only set ONE of the three parameters. They are Message ID's
        /// </summary> 
        public async Task<List<DiscordMessage>> GetMessages(ulong around = 0, ulong before = 0, ulong after = 0, int limit = 50) => await DiscordClient.InternalGetChannelMessages(ID, around, before, after, limit);
        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="MessageIDs"></param>
        /// <returns></returns>
        public async Task BulkDeleteMessages(List<ulong> MessageIDs) => await DiscordClient.InternalBulkDeleteMessages(ID, MessageIDs);
        /// <summary>
        /// Returns a list of invite objects
        /// </summary>
        /// <returns></returns>
        public async Task<List<DiscordInvite>> GetInvites() => await DiscordClient.InternalGetChannelInvites(ID);
        /// <summary>
        /// Create a new invite object
        /// </summary>
        /// <param name="MaxAge"></param>
        /// <param name="MaxUses"></param>
        /// <param name="temporary"></param>
        /// <param name="unique"></param>
        /// <returns></returns>
        public async Task<DiscordInvite> CreateInvite(int MaxAge = 86400, int MaxUses = 0, bool temporary = false, bool unique = false)
            => await DiscordClient.InternalCreateChannelInvite(ID, MaxAge, MaxUses, temporary, unique);
        /// <summary>
        /// Deletes a channel permission overwrite
        /// </summary>
        /// <param name="OverwriteID"></param>
        /// <returns></returns>
        public async Task DeleteChannelPermission(ulong OverwriteID) => await DiscordClient.InternalDeleteChannelPermission(ID, OverwriteID);
        /// <summary>
        /// Post a typing indicator
        /// </summary>
        /// <returns></returns>
        public async Task TriggerTyping() => await DiscordClient.InternalTriggerTypingIndicator(ID);
        /// <summary>
        /// Returns all pinned messages
        /// </summary>
        /// <returns></returns>
        public async Task<List<DiscordMessage>> GetPinnedMessages() => await DiscordClient.InternalGetPinnedMessages(ID);
        /// <summary>
        /// Create a new webhook
        /// </summary>
        /// <param name="name"></param>
        /// <param name="base64avatar"></param>
        /// <returns></returns>
        public async Task<DiscordWebhook> CreateWebhook(string name = "", string base64avatar = "") => await DiscordClient.InternalCreateWebhook(ID, name, base64avatar);
        /// <summary>
        /// Returns a list of webhooks
        /// </summary>
        /// <returns></returns>
        public async Task<List<DiscordWebhook>> GetWebhooks() => await DiscordClient.InternalGetChannelWebhooks(ID);

        public async Task ConnectToVoice()
        {
            if (Type == ChannelType.Text)
                throw new NotSupportedException();

            await DiscordClient.OpenVoiceConnection(this, false, false);
        }
        #endregion

    }
}
