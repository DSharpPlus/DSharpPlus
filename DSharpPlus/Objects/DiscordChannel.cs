using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a discord channel.
    /// </summary>
    public class DiscordChannel : SnowflakeObject
    {
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong GuildId { get; set; }

        /// <summary>
        /// Gets the name of this channel.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public ChannelType Type { get; internal set; }

        /// <summary>
        /// Gets the position of this channel.
        /// </summary>
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int Position { get; set; }

        /// <summary>
        /// Gets whether this channel is a DM channel.
        /// </summary>
        [JsonProperty("is_private", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPrivate { get; internal set; }

        /// <summary>
        /// Gets the guild to which this channel belongs.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild =>
            this.Discord._guilds.ContainsKey(this.GuildId) ? this.Discord._guilds[this.GuildId] : null;

        /// <summary>
        /// Gets a collection of permission overwrites for this channel.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordOverwrite> PermissionOverwrites => this._permission_overwrites_lazy.Value;
        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordOverwrite> _permission_overwrites = new List<DiscordOverwrite>();
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordOverwrite>> _permission_overwrites_lazy;

        /// <summary>
        /// Gets the channel's topic. This is applicable to text channels only.
        /// </summary>
        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; internal set; } = "";

        /// <summary>
        /// Gets the ID of the last message sent in this channel. This is applicable to text channels only.
        /// </summary>
        [JsonProperty("last_message_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong LastMessageId { get; internal set; } = 0;

        /// <summary>
        /// Gets this channel's bitrate. This is applicable to voice channels only.
        /// </summary>
        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int Bitrate { get; internal set; }

        /// <summary>
        /// Gets this channel's user limit. This is applicable to voice channels only.
        /// </summary>
        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int UserLimit { get; internal set; }

        /// <summary>
        /// Gets this channel's mention string.
        /// </summary>
        [JsonIgnore]
        public string Mention => Formatter.Mention(this);

        /// <summary>
        /// Gets whether this channel is an NSFW channel.
        /// </summary>
        [JsonIgnore]
        public bool IsNSFW => !this.IsPrivate && this.Type == ChannelType.Text && (this.Name == "nsfw" || this.Name.StartsWith("nsfw-"));
        
        /// <summary>
        /// Gets or sets the internal message cache for this channel.
        /// </summary>
        [JsonIgnore]
        internal RingBuffer<DiscordMessage> MessageCache { get; set; }

        public DiscordChannel()
        {
            this._permission_overwrites_lazy = new Lazy<IReadOnlyList<DiscordOverwrite>>(() => new ReadOnlyCollection<DiscordOverwrite>(this._permission_overwrites));
        }

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
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendFileAsync(Stream file_data, string file_name, string content = "", bool tts = false, DiscordEmbed embed = null) =>
            this.Discord._rest_client.InternalUploadFile(Id, file_data, file_name, content, tts, embed);
        /// <summary>
        /// Posts a file
        /// </summary>
        /// <param name="files"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMultipleFilesAsync(Dictionary<string, Stream> files, string content = "", bool tts = false, DiscordEmbed embed = null) =>
            this.Discord._rest_client.InternalUploadMultipleFiles(Id, files, content, tts, embed);

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
        public async Task<DiscordMessage> GetMessageAsync(ulong id)
        {
            if (this.Discord.config.MessageCacheSize > 0 && this.MessageCache.TryGet(xm => xm.Id == id, out var msg))
                return msg;

            return await this.Discord._rest_client.InternalGetMessage(Id, id);
        }

        /// <summary>
        /// Updates the channel position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Task ModifyPositionAsync(int position) =>
            this.Discord._rest_client.InternalModifyGuildChannelPosition(GuildId, Id, position);

        /// <summary>  
        /// Returns a list of messages.Only set ONE of the three parameters. They are Message ID's
        /// </summary> 
        public Task<List<DiscordMessage>> GetMessagesAsync(ulong around = 0, ulong before = 0, ulong after = 0, int limit = 50) =>
            this.Discord._rest_client.InternalGetChannelMessages(Id, around, before, after, limit);

        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        public Task DeleteMessagesAsync(IEnumerable<DiscordMessage> messages) =>
            this.Discord._rest_client.InternalBulkDeleteMessages(this.Id, messages.Where(xm => xm.Channel.Id == this.Id).Select(xm => xm.Id));

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task DeleteMessageAsync(DiscordMessage message) =>
            this.Discord._rest_client.InternalDeleteMessage(this.Id, message.Id);

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
