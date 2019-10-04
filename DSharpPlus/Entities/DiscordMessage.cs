﻿#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord text message.
    /// </summary>
    public class DiscordMessage : SnowflakeObject, IEquatable<DiscordMessage>
    {
        internal DiscordMessage()
        {
            this._attachmentsLazy = new Lazy<IReadOnlyList<DiscordAttachment>>(() => new ReadOnlyCollection<DiscordAttachment>(this._attachments));
            this._embedsLazy = new Lazy<IReadOnlyList<DiscordEmbed>>(() => new ReadOnlyCollection<DiscordEmbed>(this._embeds));
            this._mentionedChannelsLazy = new Lazy<IReadOnlyList<DiscordChannel>>(() => {
                if (this._mentionedChannels != null)
                    return new ReadOnlyCollection<DiscordChannel>(this._mentionedChannels);
                return new DiscordChannel[0];
            });
            this._mentionedRolesLazy = new Lazy<IReadOnlyList<DiscordRole>>(() => {
                if (this._mentionedRoles != null)
                    return new ReadOnlyCollection<DiscordRole>(this._mentionedRoles);
                return new DiscordRole[0];
            });
            this._mentionedUsersLazy = new Lazy<IReadOnlyList<DiscordUser>>(() => new ReadOnlyCollection<DiscordUser>(this._mentionedUsers));
            this._reactionsLazy = new Lazy<IReadOnlyList<DiscordReaction>>(() => new ReadOnlyCollection<DiscordReaction>(this._reactions));
            this._jumpLink = new Lazy<Uri>(() =>
            {
                var gid = this.Channel is DiscordDmChannel ? "@me" : this.Channel.GuildId.ToString(CultureInfo.InvariantCulture);
                var cid = this.ChannelId.ToString(CultureInfo.InvariantCulture);
                var mid = this.Id.ToString(CultureInfo.InvariantCulture);

                return new Uri($"https://discordapp.com/channels/{gid}/{cid}/{mid}");
            });
        }
        
        internal DiscordMessage(DiscordMessage other)
            : this()
        {
            this.Discord = other.Discord;

            this._attachments = other._attachments; // the attachments cannot change, thus no need to copy and reallocate.
            this._embeds = new List<DiscordEmbed>(other._embeds);
            
            if (other._mentionedChannels != null) 
                this._mentionedChannels = new List<DiscordChannel>(other._mentionedChannels);
            if (other._mentionedRoles != null)
                this._mentionedRoles = new List<DiscordRole>(other._mentionedRoles);
            this._mentionedUsers = new List<DiscordUser>(other._mentionedUsers);
            this._reactions = new List<DiscordReaction>(other._reactions);

            this.Author = other.Author;
            this.ChannelId = other.ChannelId;
            this.Content = other.Content;
            this.EditedTimestampRaw = other.EditedTimestampRaw;
            this.Id = other.Id;
            this.IsTTS = other.IsTTS;
            this.MessageType = other.MessageType;
            this.Pinned = other.Pinned;
            this.TimestampRaw = other.TimestampRaw;
            this.WebhookId = other.WebhookId;
        }

        /// <summary> 
        /// Gets the channel in which the message was sent.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel 
            => (this.Discord as DiscordClient)?.InternalGetCachedChannel(this.ChannelId);

        /// <summary>
        /// Gets ID of the channel in which the message was sent.
        /// </summary>
        [JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ChannelId { get; internal set; }

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
        public DateTimeOffset Timestamp
            => !string.IsNullOrWhiteSpace(this.TimestampRaw) && DateTimeOffset.TryParse(this.TimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ?
                dto : this.CreationTimestamp;

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string TimestampRaw { get; set; }

        /// <summary>
        /// Gets the message's edit timestamp. Will be null if the message was not edited.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? EditedTimestamp 
            => !string.IsNullOrWhiteSpace(this.EditedTimestampRaw) && DateTimeOffset.TryParse(this.EditedTimestampRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? 
                (DateTimeOffset?)dto : null;

        [JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string EditedTimestampRaw { get; set; }

        /// <summary>
        /// Gets whether this message was edited.
        /// </summary>
        [JsonIgnore]
        public bool IsEdited 
            => !string.IsNullOrWhiteSpace(this.EditedTimestampRaw);

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
        public IReadOnlyList<DiscordUser> MentionedUsers 
            => this._mentionedUsersLazy.Value;

        [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordUser> _mentionedUsers;
        [JsonIgnore]
        Lazy<IReadOnlyList<DiscordUser>> _mentionedUsersLazy;

        // TODO this will probably throw an exception in DMs since it tries to wrap around a null List...
        // this is probably low priority but need to find out a clean way to solve it...
        /// <summary>
        /// Gets roles mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordRole> MentionedRoles 
            => this._mentionedRolesLazy.Value;

        [JsonIgnore]
        internal List<DiscordRole> _mentionedRoles;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordRole>> _mentionedRolesLazy;

        /// <summary>
        /// Gets channels mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordChannel> MentionedChannels 
            => this._mentionedChannelsLazy.Value;

        [JsonIgnore]
        internal List<DiscordChannel> _mentionedChannels;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordChannel>> _mentionedChannelsLazy;

        /// <summary>
        /// Gets files attached to this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordAttachment> Attachments 
            => this._attachmentsLazy.Value;

        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordAttachment> _attachments = new List<DiscordAttachment>();
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordAttachment>> _attachmentsLazy;

        /// <summary>
        /// Gets embeds attached to this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordEmbed> Embeds 
            => this._embedsLazy.Value;

        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordEmbed> _embeds = new List<DiscordEmbed>();
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordEmbed>> _embedsLazy;

        /// <summary>
        /// Gets reactions used on this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordReaction> Reactions 
            => this._reactionsLazy.Value;

        [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordReaction> _reactions = new List<DiscordReaction>();
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordReaction>> _reactionsLazy;

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
        /// Gets whether the message originated from a webhook.
        /// </summary>
        [JsonIgnore]
        public bool WebhookMessage 
            => this.WebhookId != null;

        /// <summary>
        /// Gets the jump link to this message.
        /// </summary>
        [JsonIgnore]
        public Uri JumpLink => this._jumpLink.Value;
        private Lazy<Uri> _jumpLink;

        /// <summary>
        /// Edits the message.
        /// </summary>
        /// <param name="content">New content.</param>
        /// <param name="embed">New embed.</param>
        /// <returns></returns>
        public Task<DiscordMessage> ModifyAsync(Optional<string> content = default, Optional<DiscordEmbed> embed = default) 
            => this.Discord.ApiClient.EditMessageAsync(this.ChannelId, this.Id, content, embed);

        /// <summary>
        /// Deletes the message.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) 
            => this.Discord.ApiClient.DeleteMessageAsync(this.ChannelId, this.Id, reason);

        /// <summary>
        /// Removes all embeds in the message.
        /// </summary>
        /// <param name="suppress">Whether to suppress all embeds.</param>
        /// <returns></returns>
        public Task SuppressEmbedsAsync(bool suppress = true)
            => this.Discord.ApiClient.SuppressEmbedsAsync(suppress, this.ChannelId, this.Id);

        /// <summary>
        /// Pins the message in its channel.
        /// </summary>
        /// <returns></returns>
        public Task PinAsync() 
            => this.Discord.ApiClient.PinMessageAsync(this.ChannelId, Id);

        /// <summary>
        /// Unpins the message in its channel.
        /// </summary>
        /// <returns></returns>
        public Task UnpinAsync() 
            => this.Discord.ApiClient.UnpinMessageAsync(this.ChannelId, Id);

        /// <summary>
        /// Responds to the message.
        /// </summary>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondAsync(string content = null, bool tts = false, DiscordEmbed embed = null) 
            => this.Discord.ApiClient.CreateMessageAsync(this.ChannelId, content, tts, embed);

        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="fileName">Name of the file to be attached.</param>
        /// <param name="fileData">Stream containing the data to attach to the message as a file.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(string fileName, Stream fileData, string content = null, bool tts = false, DiscordEmbed embed = null) 
            => this.Discord.ApiClient.UploadFileAsync(this.ChannelId, fileData, fileName, content, tts, embed);

#if !NETSTANDARD1_1
        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach to the message as a file.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(FileStream fileData, string content = null, bool tts = false, DiscordEmbed embed = null) 
            => this.Discord.ApiClient.UploadFileAsync(this.ChannelId, fileData, Path.GetFileName(fileData.Name), content, tts, embed);

        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="filePath">Path to the file to be attached to the message.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> RespondWithFileAsync(string filePath, string content = null, bool tts = false, DiscordEmbed embed = null)
        {
            using (var fs = File.OpenRead(filePath))
                return await this.Discord.ApiClient.UploadFileAsync(this.ChannelId, fs, Path.GetFileName(fs.Name), content, tts, embed).ConfigureAwait(false);
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
        public Task<DiscordMessage> RespondWithFilesAsync(Dictionary<string, Stream> files, string content = null, bool tts = false, DiscordEmbed embed = null) 
            => this.Discord.ApiClient.UploadFilesAsync(ChannelId, files, content, tts, embed);

        /// <summary>
        /// Creates a reaction to this message
        /// </summary>
        /// <param name="emoji">The emoji you want to react with, either an emoji or name:id</param>
        /// <returns></returns>
        public Task CreateReactionAsync(DiscordEmoji emoji) 
            => this.Discord.ApiClient.CreateReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes your own reaction
        /// </summary>
        /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
        /// <returns></returns>
        public Task DeleteOwnReactionAsync(DiscordEmoji emoji) 
            => this.Discord.ApiClient.DeleteOwnReactionAsync(this.ChannelId, this.Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes another user's reaction.
        /// </summary>
        /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
        /// <param name="user">Member you want to remove the reaction for</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteReactionAsync(DiscordEmoji emoji, DiscordUser user, string reason = null) 
            => this.Discord.ApiClient.DeleteUserReactionAsync(this.ChannelId, this.Id, user.Id, emoji.ToReactionString(), reason);

        /// <summary>
        /// Gets users that reacted with this emoji
        /// </summary>
        /// <param name="emoji">Emoji to react with.</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(DiscordEmoji emoji) 
            => this.Discord.ApiClient.GetReactionsAsync(this.Channel.Id, this.Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes all reactions for this message
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteAllReactionsAsync(string reason = null) 
            => this.Discord.ApiClient.DeleteAllReactionsAsync(this.Channel.Id, this.Id, reason);
        
        /// <summary>
        /// Returns a string representation of this message.
        /// </summary>
        /// <returns>String representation of this message.</returns>
        public override string ToString()
        {
            return $"Message {this.Id}; Attachment count: {this._attachments.Count}; Embed count: {this._embeds.Count}; Contents: {this.Content}";
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMessage"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordMessage"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordMessage);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMessage"/> is equal to another <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordMessage"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordMessage"/> is equal to this <see cref="DiscordMessage"/>.</returns>
        public bool Equals(DiscordMessage e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id && this.ChannelId == e.ChannelId;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordMessage"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordMessage"/>.</returns>
        public override int GetHashCode()
        {
            int hash = 13;

            hash = (hash * 7) + this.Id.GetHashCode();
            hash = (hash * 7) + this.ChannelId.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordMessage"/> objects are equal.
        /// </summary>
        /// <param name="e1">First message to compare.</param>
        /// <param name="e2">Second message to compare.</param>
        /// <returns>Whether the two messages are equal.</returns>
        public static bool operator ==(DiscordMessage e1, DiscordMessage e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id && e1.ChannelId == e2.ChannelId;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordMessage"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First message to compare.</param>
        /// <param name="e2">Second message to compare.</param>
        /// <returns>Whether the two messages are not equal.</returns>
        public static bool operator !=(DiscordMessage e1, DiscordMessage e2) 
            => !(e1 == e2);
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

        /// <summary>
        /// USER pinned a message to this channel.
        /// </summary>
        ChannelPinnedMessage = 6,

        /// <summary>
        /// Message when a guild member joins. Most frequently seen in newer, smaller guilds.
        /// </summary>
        GuildMemberJoin = 7
    }
}
