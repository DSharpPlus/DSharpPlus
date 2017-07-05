using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a Discord text message.
    /// </summary>
    public class DiscordMessage : SnowflakeObject
    {
        public DiscordMessage()
        {
            this._attachments_lazy = new Lazy<IReadOnlyList<DiscordAttachment>>(() => new ReadOnlyCollection<DiscordAttachment>(this._attachments));
            this._embeds_lazy = new Lazy<IReadOnlyList<DiscordEmbed>>(() => new ReadOnlyCollection<DiscordEmbed>(this._embeds));
            this._mentioned_channels_lazy = new Lazy<IReadOnlyList<DiscordChannel>>(() => new ReadOnlyCollection<DiscordChannel>(this._mentioned_channels));
            this._mentioned_roles_lazy = new Lazy<IReadOnlyList<DiscordRole>>(() => new ReadOnlyCollection<DiscordRole>(this._mentioned_roles));
            this._mentioned_users_lazy = new Lazy<IReadOnlyList<DiscordUser>>(() => new ReadOnlyCollection<DiscordUser>(this._mentioned_users));
            this._reactions_lazy = new Lazy<IReadOnlyList<DiscordReaction>>(() => new ReadOnlyCollection<DiscordReaction>(this._reactions));
        }

        /// <summary> 
        /// Gets the channel in which the message was sent.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel =>
            this.Discord.InternalGetCachedChannel(this.ChannelId);
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong ChannelId { get; set; }

        /// <summary>
        /// Gets the user or member that sent the message.
        /// </summary>
        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Author { get; internal set; }

        /// <summary>
        /// Gets the message's content.
        /// </summary>
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; internal set; }

        /// <summary>
        /// Gets the message's creation timestamp.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset Timestamp => DateTimeOffset.Parse(this.TimestampRaw);
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string TimestampRaw { get; set; }

        /// <summary>
        /// Gets the message's edit timestamp.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset EditedTimestamp => DateTimeOffset.Parse(this.EditedTimestampRaw);
        [JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string EditedTimestampRaw { get; set; }

        /// <summary>
        /// Gets whether the message is a text-to-speech message.
        /// </summary>
        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsTTS { get; internal set; }

        /// <summary>
        /// Gets whether the message mentions everyone.
        /// </summary>
        [JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
        public bool MentionEveryone { get; internal set; }

        /// <summary>
        /// Gets users or members mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordUser> MentionedUsers => this._mentioned_users_lazy.Value;
        [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordUser> _mentioned_users;
        [JsonIgnore]
        Lazy<IReadOnlyList<DiscordUser>> _mentioned_users_lazy;

        /// <summary>
        /// Gets roles mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordRole> MentionedRoles => this._mentioned_roles_lazy.Value;
        [JsonIgnore]
        internal List<DiscordRole> _mentioned_roles;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordRole>> _mentioned_roles_lazy;

        /// <summary>
        /// Gets channels mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordChannel> MentionedChannels => this._mentioned_channels_lazy.Value;
        [JsonIgnore]
        internal List<DiscordChannel> _mentioned_channels;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordChannel>> _mentioned_channels_lazy;

        /// <summary>
        /// Gets files attached to this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordAttachment> Attachments => this._attachments_lazy.Value;
        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordAttachment> _attachments;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordAttachment>> _attachments_lazy;

        /// <summary>
        /// Gets embeds attached to this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordEmbed> Embeds => this._embeds_lazy.Value;
        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordEmbed> _embeds;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordEmbed>> _embeds_lazy;

        /// <summary>
        /// Gets reactions used on this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordReaction> Reactions => this._reactions_lazy.Value;
        [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordReaction> _reactions;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordReaction>> _reactions_lazy;

        /*
        /// <summary>
        /// Gets the nonce sent with the message, if the message was sent by the client.
        /// </summary>
        [JsonProperty("nonce", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? Nonce { get; internal set; }
        */

        /// <summary>
        /// Gets whether the message is pinned.
        /// </summary>
        [JsonProperty("pinned", NullValueHandling = NullValueHandling.Ignore)]
        public bool Pinned { get; internal set; }

        /// <summary>
        /// Gets the id of the webhook that generated this message.
        /// </summary>
        [JsonProperty("webhook_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? WebhookId { get; internal set; }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public MessageType? MessageType { get; internal set; }

        /// <summary>
        /// Edits the message.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> EditAsync(string content = null, DiscordEmbed embed = null) =>
            this.Discord._rest_client.InternalEditMessageAsync(ChannelId, Id, content, embed);

        /// <summary>
        /// Deletes the message,
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) =>
            this.Discord._rest_client.InternalDeleteMessageAsync(this.ChannelId, this.Id, reason);

        /// <summary>
        /// Pins the message in its channel.
        /// </summary>
        /// <returns></returns>
        public Task PinAsync() =>
            this.Discord._rest_client.InternalPinMessageAsync(ChannelId, Id);

        /// <summary>
        /// Unpins the message in its channel.
        /// </summary>
        /// <returns></returns>
        public Task UnpinAsync() =>
            this.Discord._rest_client.InternalUnpinMessageAsync(ChannelId, Id);

        /// <summary>
        /// Responds to the message.
        /// </summary>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondAsync(string content, bool tts = false, DiscordEmbed embed = null) =>
            this.Discord._rest_client.InternalCreateMessageAsync(ChannelId, content, tts, embed);

        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach to the message as a file.</param>
        /// <param name="file_name">Name of the file to be attached.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(Stream file_data, string file_name, string content = null, bool tts = false, DiscordEmbed embed = null) => 
            this.Discord._rest_client.InternalUploadFileAsync(ChannelId, file_data, file_name, content, tts, embed);

#if !NETSTANDARD1_1
        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach to the message as a file.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(FileStream file_data, string content = null, bool tts = false, DiscordEmbed embed = null) =>
            this.RespondWithFileAsync(file_data, Path.GetFileName(file_data.Name), content, tts, embed);

        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="file_path">Path to the file to be attached to the message.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(string file_path, string content = null, bool tts = false, DiscordEmbed embed = null)
        {
            using (var fs = File.OpenRead(file_path))
                return this.RespondWithFileAsync(fs, content, tts, embed);
        }
#endif

        /// <summary>
        /// Responds to the message with several files.
        /// </summary>
        /// <param name="files">A filename to data stream mapping.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFilesAsync(Dictionary<string, Stream> files, string content = null, bool tts = false, DiscordEmbed embed = null) =>
            this.Discord._rest_client.InternalUploadFilesAsync(ChannelId, files, content, tts, embed);

        /// <summary>
        /// Creates a reaction to this message
        /// </summary>
        /// <param name="emoji">The emoji you want to react with, either an emoji or name:id</param>
        /// <returns></returns>
        public Task CreateReactionAsync(DiscordEmoji emoji) =>
            this.Discord._rest_client.InternalCreateReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes your own reaction
        /// </summary>
        /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
        /// <returns></returns>
        public Task DeleteOwnReactionAsync(DiscordEmoji emoji) =>
            this.Discord._rest_client.InternalDeleteOwnReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes another user's reaction.
        /// </summary>
        /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
        /// <param name="user">Member you want to remove the reaction for</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteReactionAsync(DiscordEmoji emoji, DiscordUser user, string reason = null) =>
            this.Discord._rest_client.InternalDeleteUserReactionAsync(this.ChannelId, this.Id, user.Id, emoji.ToReactionString(), reason);

        /// <summary>
        /// Gets users that reacted with this emoji
        /// </summary>
        /// <param name="emoji">Emoji to react with.</param>
        /// <returns></returns>
        public Task<IReadOnlyCollection<DiscordUser>> GetReactionsAsync(DiscordEmoji emoji) =>
            this.Discord._rest_client.InternalGetReactionsAsync(this.Channel.Id, this.Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes all reactions for this message
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteAllReactionsAsync(string reason = null) =>
            this.Discord._rest_client.InternalDeleteAllReactionsAsync(this.Channel.Id, this.Id, reason);

        /// <summary>
        /// Returns a string representation of this message.
        /// </summary>
        /// <returns>String representation of this message.</returns>
        public override string ToString()
        {
            return string.Concat("Message ", this.Id, "; Attachment count: ", this._attachments.Count, "; Embed count: ", this._embeds.Count, "; Contents: ", this.Content);
        }
    }

    /// <summary>
    /// Indicates the type of the message.
    /// </summary>
    public enum MessageType : int
    {
        /// <summary>
        /// Indicates a regular message.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Message indicating a recipient was added to a group direct message.
        /// </summary>
        RecipientAdd = 1,

        /// <summary>
        /// Message indicating a recipient was removed from a group direct message.
        /// </summary>
        RecipientRemove = 2,

        /// <summary>
        /// Message indicating a call.
        /// </summary>
        Call = 3,

        /// <summary>
        /// Message indicating a group direct message channel rename.
        /// </summary>
        ChannelNameChange = 4,

        /// <summary>
        /// Message indicating a group direct message channel icon change.
        /// </summary>
        ChannelIconChange = 5,
    }
}
