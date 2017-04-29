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
        public DiscordGuild Guild => (GuildID == 0) ? new DiscordGuild() : (this.Discord._guilds.ContainsKey(GuildID)) ? this.Discord._guilds[GuildID] : this.Discord._rest_client.InternalGetGuildAsync(GuildID).GetAwaiter().GetResult();
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

        #region Methods
        /// <summary>
        /// Posts a message
        /// </summary>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMessageAsync(string content, bool tts = false, DiscordEmbed embed = null) =>
            this.Discord._rest_client.InternalCreateMessage(Id, content, tts, embed);
        /// <summary>
        /// Posts a file
        /// </summary>
        /// <param name="file_data"></param>
        /// <param name="file_name"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendFileAsync(Stream file_data, string file_name, string content = "", bool tts = false) =>
            this.Discord._rest_client.InternalUploadFile(Id, file_data, file_name, content, tts);
        /// <summary>
        /// Posts a file
        /// </summary>
        /// <param name="files"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMultipleFilesAsync(Dictionary<string, Stream> files, string content = "", bool tts = false) =>
            this.Discord._rest_client.InternalUploadMultipleFiles(Id, files, content, tts);

        // Please send memes to Naamloos#2887 at discord <3 thank you

        /// <summary>
        /// Deletes a guild channel
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync() =>
            this.Discord._rest_client.InternalDeleteChannel(Id);
        /// <summary>
        /// Returns a specific message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DiscordMessage> GetMessageAsync(ulong id) =>
            this.Discord._rest_client.InternalGetMessage(Id, id);
        /// <summary>
        /// Updates the channel position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Task ModifyPositionAsync(int position) =>
            this.Discord._rest_client.InternalModifyGuildChannelPosition(GuildID, Id, position);
        /// <summary>  
        /// Returns a list of messages.Only set ONE of the three parameters. They are Message ID's
        /// </summary> 
        public Task<List<DiscordMessage>> GetMessagesAsync(ulong around = 0, ulong before = 0, ulong after = 0, int limit = 50) =>
            this.Discord._rest_client.InternalGetChannelMessages(Id, around, before, after, limit);
        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="message_ids"></param>
        /// <returns></returns>
        public Task BulkDeleteMessagesAsync(List<ulong> message_ids) =>
            this.Discord._rest_client.InternalBulkDeleteMessages(Id, message_ids);
        /// <summary>
        /// Returns a list of invite objects
        /// </summary>
        /// <returns></returns>
        public Task<List<DiscordInvite>> GetInvitesAsync() =>
            this.Discord._rest_client.InternalGetChannelInvites(Id);
        /// <summary>
        /// Create a new invite object
        /// </summary>
        /// <param name="max_age"></param>
        /// <param name="max_uses"></param>
        /// <param name="temporary"></param>
        /// <param name="unique"></param>
        /// <returns></returns>
        public Task<DiscordInvite> CreateInviteAsync(int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false) => 
            this.Discord._rest_client.InternalCreateChannelInvite(Id, max_age, max_uses, temporary, unique);
        /// <summary>
        /// Deletes a channel permission overwrite
        /// </summary>
        /// <param name="overwrite_id"></param>
        /// <returns></returns>
        public Task DeleteChannelPermissionAsync(ulong overwrite_id) =>
            this.Discord._rest_client.InternalDeleteChannelPermission(Id, overwrite_id);
        /// <summary>
        /// Post a typing indicator
        /// </summary>
        /// <returns></returns>
        public Task TriggerTypingAsync() =>
            this.Discord._rest_client.InternalTriggerTypingIndicator(Id);
        /// <summary>
        /// Returns all pinned messages
        /// </summary>
        /// <returns></returns>
        public Task<List<DiscordMessage>> GetPinnedMessagesAsync() =>
            this.Discord._rest_client.InternalGetPinnedMessages(Id);
        /// <summary>
        /// Create a new webhook
        /// </summary>
        /// <param name="name"></param>
        /// <param name="base64_avatar"></param>
        /// <returns></returns>
        public Task<DiscordWebhook> CreateWebhookAsync(string name = "", string base64_avatar = "") =>
            this.Discord._rest_client.InternalCreateWebhook(Id, name, base64_avatar);
        /// <summary>
        /// Returns a list of webhooks
        /// </summary>
        /// <returns></returns>
        public Task<List<DiscordWebhook>> GetWebhooksAsync() =>
            this.Discord._rest_client.InternalGetChannelWebhooks(Id);

        /// <summary>
        /// Moves a member to this voice channel
        /// </summary>
        /// <param name="member_id"></param>
        /// <returns></returns>
        public async Task PlaceMemberAsync(ulong member_id)
        {
            if (Type == ChannelType.Voice)
                await this.Discord._rest_client.InternalModifyGuildMember(Guild.Id, member_id, voicechannel_id: Id);
        }

        public Task UpdateOverwriteAsync(DiscordOverwrite overwrite) =>
            this.Discord._rest_client.InternalEditChannelPermissions(Id, overwrite.Id, overwrite.Allow, overwrite.Deny, overwrite.Type);
        #endregion

    }
}
