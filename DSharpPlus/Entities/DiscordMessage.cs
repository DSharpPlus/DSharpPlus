using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using DSharpPlus.Enums;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord text message.
    /// </summary>
    public class DiscordMessage : SnowflakeObject, IEquatable<DiscordMessage>
    {
        internal DiscordMessage()
        {
            _attachmentsLazy = new Lazy<IReadOnlyList<DiscordAttachment>>(() => new ReadOnlyCollection<DiscordAttachment>(_attachments));
            _embedsLazy = new Lazy<IReadOnlyList<DiscordEmbed>>(() => new ReadOnlyCollection<DiscordEmbed>(_embeds));
            _mentionedChannelsLazy = new Lazy<IReadOnlyList<DiscordChannel>>(() => new ReadOnlyCollection<DiscordChannel>(_mentionedChannels));
            _mentionedRolesLazy = new Lazy<IReadOnlyList<DiscordRole>>(() => new ReadOnlyCollection<DiscordRole>(_mentionedRoles));
            _mentionedUsersLazy = new Lazy<IReadOnlyList<DiscordUser>>(() => new ReadOnlyCollection<DiscordUser>(_mentionedUsers));
            _reactionsLazy = new Lazy<IReadOnlyList<DiscordReaction>>(() => new ReadOnlyCollection<DiscordReaction>(_reactions));
        }

        /// <summary> 
        /// Gets the channel in which the message was sent.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Channel =>
            (Discord as DiscordClient)?.InternalGetCachedChannel(ChannelId);

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
        public DateTimeOffset Timestamp => DateTimeOffset.Parse(TimestampRaw, CultureInfo.InvariantCulture);
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string TimestampRaw { get; set; }

        /// <summary>
        /// Gets the message's edit timestamp.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset EditedTimestamp => DateTimeOffset.Parse(EditedTimestampRaw, CultureInfo.InvariantCulture);
        [JsonProperty("edited_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        internal string EditedTimestampRaw { get; set; }
        [JsonIgnore]
        public bool IsEdited => !string.IsNullOrWhiteSpace(EditedTimestampRaw);

        /// <summary>
        /// Gets whether the message is a text-to-speech message.
        /// </summary>
        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsTts { get; internal set; }

        /// <summary>
        /// Gets whether the message mentions everyone.
        /// </summary>
        [JsonProperty("mention_everyone", NullValueHandling = NullValueHandling.Ignore)]
        public bool MentionEveryone { get; internal set; }

        /// <summary>
        /// Gets users or members mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordUser> MentionedUsers => _mentionedUsersLazy.Value;
        [JsonProperty("mentions", NullValueHandling = NullValueHandling.Ignore)]
        // ReSharper disable once InconsistentNaming
        internal List<DiscordUser> _mentionedUsers;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<DiscordUser>> _mentionedUsersLazy;

        /// <summary>
        /// Gets roles mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordRole> MentionedRoles => _mentionedRolesLazy.Value;
        [JsonIgnore]
        // ReSharper disable once InconsistentNaming
        internal List<DiscordRole> _mentionedRoles;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<DiscordRole>> _mentionedRolesLazy;

        /// <summary>
        /// Gets channels mentioned by this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordChannel> MentionedChannels => _mentionedChannelsLazy.Value;
        [JsonIgnore]
        // ReSharper disable once InconsistentNaming
        internal List<DiscordChannel> _mentionedChannels;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<DiscordChannel>> _mentionedChannelsLazy;

        /// <summary>
        /// Gets files attached to this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordAttachment> Attachments => _attachmentsLazy.Value;
        [JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
        // ReSharper disable once InconsistentNaming
        internal List<DiscordAttachment> _attachments;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<DiscordAttachment>> _attachmentsLazy;

        /// <summary>
        /// Gets embeds attached to this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordEmbed> Embeds => _embedsLazy.Value;
        [JsonProperty("embeds", NullValueHandling = NullValueHandling.Ignore)]
        // ReSharper disable once InconsistentNaming
        internal List<DiscordEmbed> _embeds;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<DiscordEmbed>> _embedsLazy;

        /// <summary>
        /// Gets reactions used on this message.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordReaction> Reactions => _reactionsLazy.Value;
        [JsonProperty("reactions", NullValueHandling = NullValueHandling.Ignore)]
        // ReSharper disable once InconsistentNaming
        internal List<DiscordReaction> _reactions;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<DiscordReaction>> _reactionsLazy;

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
        public bool WebhookMessage => WebhookId != null;

        /// <summary>
        /// Edits the message.
        /// </summary>
        /// <param name="content">New content.</param>
        /// <param name="embed">New embed.</param>
        /// <returns></returns>
        public Task<DiscordMessage> ModifyAsync(Optional<string> content = default, Optional<DiscordEmbed> embed = default(Optional<DiscordEmbed>)) =>
            Discord.ApiClient.EditMessageAsync(ChannelId, Id, content, embed);

        /// <summary>
        /// Deletes the message.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) =>
            Discord.ApiClient.DeleteMessageAsync(ChannelId, Id, reason);

        /// <summary>
        /// Pins the message in its channel.
        /// </summary>
        /// <returns></returns>
        public Task PinAsync() =>
            Discord.ApiClient.PinMessageAsync(ChannelId, Id);

        /// <summary>
        /// Unpins the message in its channel.
        /// </summary>
        /// <returns></returns>
        public Task UnpinAsync() =>
            Discord.ApiClient.UnpinMessageAsync(ChannelId, Id);

        /// <summary>
        /// Responds to the message.
        /// </summary>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondAsync(string content = null, bool tts = false, DiscordEmbed embed = null) =>
            Discord.ApiClient.CreateMessageAsync(ChannelId, content, tts, embed);

        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach to the message as a file.</param>
        /// <param name="fileName">Name of the file to be attached.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(Stream fileData, string fileName, string content = null, bool tts = false, DiscordEmbed embed = null) => 
            Discord.ApiClient.UploadFileAsync(ChannelId, fileData, fileName, content, tts, embed);

#if !NETSTANDARD1_1
        /// <summary>
        /// Responds to the message with a file.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach to the message as a file.</param>
        /// <param name="content">Message content to respond with.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> RespondWithFileAsync(FileStream fileData, string content = null, bool tts = false, DiscordEmbed embed = null) =>
            Discord.ApiClient.UploadFileAsync(ChannelId, fileData, Path.GetFileName(fileData.Name), content, tts, embed);

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
            {
                return await Discord.ApiClient.UploadFileAsync(ChannelId, fs, Path.GetFileName(fs.Name), content, tts, embed);
            }
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
            Discord.ApiClient.UploadFilesAsync(ChannelId, files, content, tts, embed);

        /// <summary>
        /// Creates a reaction to this message
        /// </summary>
        /// <param name="emoji">The emoji you want to react with, either an emoji or name:id</param>
        /// <returns></returns>
        public Task CreateReactionAsync(DiscordEmoji emoji) =>
            Discord.ApiClient.CreateReactionAsync(ChannelId, Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes your own reaction
        /// </summary>
        /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
        /// <returns></returns>
        public Task DeleteOwnReactionAsync(DiscordEmoji emoji) =>
            Discord.ApiClient.DeleteOwnReactionAsync(ChannelId, Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes another user's reaction.
        /// </summary>
        /// <param name="emoji">Emoji for the reaction you want to remove, either an emoji or name:id</param>
        /// <param name="user">Member you want to remove the reaction for</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteReactionAsync(DiscordEmoji emoji, DiscordUser user, string reason = null) =>
            Discord.ApiClient.DeleteUserReactionAsync(ChannelId, Id, user.Id, emoji.ToReactionString(), reason);

        /// <summary>
        /// Gets users that reacted with this emoji
        /// </summary>
        /// <param name="emoji">Emoji to react with.</param>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(DiscordEmoji emoji) =>
            Discord.ApiClient.GetReactionsAsync(Channel.Id, Id, emoji.ToReactionString());

        /// <summary>
        /// Deletes all reactions for this message
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteAllReactionsAsync(string reason = null) =>
            Discord.ApiClient.DeleteAllReactionsAsync(Channel.Id, Id, reason);

        /// <summary>
        /// Acknowledges the message. This is available to user tokens only.
        /// </summary>
        /// <returns></returns>
        public Task AcknowledgeAsync()
        {
            if (Discord.Configuration.TokenType == TokenType.User)
            {
                return Discord.ApiClient.AcknowledgeMessageAsync(Id, Channel.Id);
            }

            throw new InvalidOperationException("ACK can only be used when logged in as regular user.");
        }
        
        /// <summary>
        /// Returns a string representation of this message.
        /// </summary>
        /// <returns>String representation of this message.</returns>
        public override string ToString()
        {
            return string.Concat("Message ", Id, "; Attachment count: ", _attachments.Count, "; Embed count: ", _embeds.Count, "; Contents: ", Content);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMessage"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordMessage"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as DiscordMessage);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMessage"/> is equal to another <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordMessage"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordMessage"/> is equal to this <see cref="DiscordMessage"/>.</returns>
        public bool Equals(DiscordMessage e)
        {
            if (ReferenceEquals(e, null))
            {
                return false;
            }

            if (ReferenceEquals(this, e))
            {
                return true;
            }

            return Id == e.Id && ChannelId == e.ChannelId;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordMessage"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordMessage"/>.</returns>
        public override int GetHashCode()
        {
            var hash = 13;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            hash = hash * 7 + Id.GetHashCode();
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            hash = hash * 7 + ChannelId.GetHashCode();

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

            if (o1 == null && o2 != null || o1 != null && o2 == null)
            {
                return false;
            }

            if (o1 == null)
            {
                return true;
            }

            return e1.Id == e2.Id && e1.ChannelId == e2.ChannelId;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordMessage"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First message to compare.</param>
        /// <param name="e2">Second message to compare.</param>
        /// <returns>Whether the two messages are not equal.</returns>
        public static bool operator !=(DiscordMessage e1, DiscordMessage e2) =>
            !(e1 == e2);
    }

    /// <summary>
    /// Indicates the type of the message.
    /// </summary>
    public enum MessageType
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
