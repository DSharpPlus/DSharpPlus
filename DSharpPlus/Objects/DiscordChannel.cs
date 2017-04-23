using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildID { get; internal set; }
        /// <summary>
        /// The name of the channel
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }
        /// <summary>
        /// The type of the channel
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public ChannelType Type { get; internal set; }
        /// <summary>
        /// Sorting position of the channel
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; set; }
        /// <summary>
        /// Should always be false for guild channels
        /// </summary>
        [JsonProperty("is_private", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrivate { get; internal set; }
        /// <summary>
        /// The guild
        /// </summary>
        public DiscordGuild Parent => (GuildID == 0) ? new DiscordGuild() : (DiscordClient._guilds.ContainsKey(GuildID)) ? DiscordClient._guilds[GuildID] : DiscordClient.InternalGetGuildAsync(GuildID).Result;
        /// <summary>
        /// A list of permission overwrite
        /// </summary>
        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
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
        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int Bitrate { get; internal set; }
        /// <summary>
        /// The channel user limit (Voice only)
        /// </summary>
        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int UserLimit { get; internal set; }
        /// <summary>
        /// Mentions the channel similar to how a client would
        /// </summary>
        public string Mention => Formatter.Mention(this);

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
        /// <param name="file_data"></param>
        /// <param name="file_name"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> SendFile(Stream file_data, string file_name, string content = "", bool tts = false) => await DiscordClient.InternalUploadFile(ID, file_data, file_name, content, tts);
        /// <summary>
        /// Deletes a guild channel
        /// </summary>
        /// <returns></returns>
        public async Task Delete() => await DiscordClient.InternalDeleteChannel(ID);
        /// <summary>
        /// Returns a specific message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessage(ulong id) => await DiscordClient.InternalGetMessage(ID, id);
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
        /// <param name="message_ids"></param>
        /// <returns></returns>
        public async Task BulkDeleteMessages(List<ulong> message_ids) => await DiscordClient.InternalBulkDeleteMessages(ID, message_ids);
        /// <summary>
        /// Returns a list of invite objects
        /// </summary>
        /// <returns></returns>
        public async Task<List<DiscordInvite>> GetInvites() => await DiscordClient.InternalGetChannelInvites(ID);
        /// <summary>
        /// Create a new invite object
        /// </summary>
        /// <param name="max_age"></param>
        /// <param name="max_uses"></param>
        /// <param name="temporary"></param>
        /// <param name="unique"></param>
        /// <returns></returns>
        public async Task<DiscordInvite> CreateInvite(int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false)
            => await DiscordClient.InternalCreateChannelInvite(ID, max_age, max_uses, temporary, unique);
        /// <summary>
        /// Deletes a channel permission overwrite
        /// </summary>
        /// <param name="overwrite_id"></param>
        /// <returns></returns>
        public async Task DeleteChannelPermission(ulong overwrite_id) => await DiscordClient.InternalDeleteChannelPermission(ID, overwrite_id);
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
        /// <param name="base64_avatar"></param>
        /// <returns></returns>
        public async Task<DiscordWebhook> CreateWebhook(string name = "", string base64_avatar = "") => await DiscordClient.InternalCreateWebhook(ID, name, base64_avatar);
        /// <summary>
        /// Returns a list of webhooks
        /// </summary>
        /// <returns></returns>
        public async Task<List<DiscordWebhook>> GetWebhooks() => await DiscordClient.InternalGetChannelWebhooks(ID);

        /// <summary>
        /// Moves a member to this voice channel
        /// </summary>
        /// <param name="member_id"></param>
        /// <returns></returns>
        public async Task PlaceMember(ulong member_id)
        {
            if (Type == ChannelType.Voice)
            {
                await DiscordClient.InternalModifyGuildMember(Parent.ID, member_id, voicechannel_id: ID);
            }
        }

        public async Task UpdateOverwrite(DiscordOverwrite overwrite) =>
            await DiscordClient.InternalEditChannelPermissions(ID, overwrite.ID, overwrite.Allow, overwrite.Deny, overwrite.Type);
        #endregion

    }
}
