using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a discord channel.
    /// </summary>
    public class DiscordChannel : SnowflakeObject, IEquatable<DiscordChannel>
    {
        /// <summary>
        /// Gets ID of the guild to which this channel belongs.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets ID of the category that contains this channel.
        /// </summary>
        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Include)]
        public ulong? ParentId { get; internal set; }

        /// <summary>
        /// Gets the category that contains this channel.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel Parent =>
            this.Guild.Channels.FirstOrDefault(xc => xc.Id == this.ParentId);

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
        [JsonIgnore]
        public bool IsPrivate => this.Type == ChannelType.Private || this.Type == ChannelType.Group;

        /// <summary>
        /// Gets whether this channel is a channel category.
        /// </summary>
        [JsonIgnore]
        public bool IsCategory => this.Type == ChannelType.Category;

        /// <summary>
        /// Gets the guild to which this channel belongs.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild =>
            this.Discord.Guilds.ContainsKey(this.GuildId) ? this.Discord.Guilds[this.GuildId] : null;

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
        /// Gets this channel's children. This applies only to channel categories.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordChannel> Children
        {
            get
            {
                if (!IsCategory)
                    throw new ArgumentException("Only channel categories contain children");

                return Guild._channels.Where(e => e.ParentId == Id);
            }
        }

        /// <summary>
        /// Gets whether this channel is an NSFW channel.
        /// </summary>
        [JsonProperty("nsfw")]
        public bool IsNSFW { get; internal set; }

        internal DiscordChannel()
        {
            this._permission_overwrites_lazy = new Lazy<IReadOnlyList<DiscordOverwrite>>(() => new ReadOnlyCollection<DiscordOverwrite>(this._permission_overwrites));
        }

        #region Methods
        /// <summary>
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendMessageAsync(string content = null, bool tts = false, DiscordEmbed embed = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("Cannot send a file to a non-text channel");

            return this.Discord.ApiClient.CreateMessageAsync(Id, content, tts, embed);
        }

        /// <summary>
        /// Sends a message containing an attached file to this channel.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach to the message as a file.</param>
        /// <param name="file_name">Name of the file to attach to the message.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendFileAsync(Stream file_data, string file_name, string content = null, bool tts = false, DiscordEmbed embed = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("Cannot send a file to a non-text channel");

            return this.Discord.ApiClient.UploadFileAsync(this.Id, file_data, file_name, content, tts, embed);
        }

#if !NETSTANDARD1_1
        /// <summary>
        /// Sends a message containing an attached file to this channel.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach to the message as a file.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendFileAsync(FileStream file_data, string content = null, bool tts = false, DiscordEmbed embed = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("Cannot send a file to a non-text channel");

            return this.Discord.ApiClient.UploadFileAsync(this.Id, file_data, Path.GetFileName(file_data.Name), content,
                tts, embed);
        }

        /// <summary>
        /// Sends a message containing an attached file to this channel.
        /// </summary>
        /// <param name="file_path">Path to the file to be attached to the message.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(string file_path, string content = null, bool tts = false, DiscordEmbed embed = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("Cannot send a file to a non-text channel");

            using (var fs = File.OpenRead(file_path))
                return await this.Discord.ApiClient.UploadFileAsync(this.Id, fs, Path.GetFileName(fs.Name), content, tts, embed);
        }
#endif

        /// <summary>
        /// Sends a message with several attached files to this channel.
        /// </summary>
        /// <param name="files">A filename to data stream mapping.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendMultipleFilesAsync(Dictionary<string, Stream> files, string content = "", bool tts = false, DiscordEmbed embed = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("Cannot send a file to a non-text channel");

            return this.Discord.ApiClient.UploadFilesAsync(Id, files, content, tts, embed);
        }

        // Please send memes to Naamloos#2887 at discord <3 thank you

        /// <summary>
        /// Deletes a guild channel
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) =>
            this.Discord.ApiClient.DeleteChannelAsync(Id, reason);

        /// <summary>
        /// Returns a specific message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessageAsync(ulong id)
        {
            if (this.Discord.Configuration.MessageCacheSize > 0 && this.Discord is DiscordClient dc && dc.MessageCache.TryGet(xm => xm.Id == id && xm.ChannelId == this.Id, out var msg))
                return msg;

            return await this.Discord.ApiClient.GetMessageAsync(Id, id);
        }

        /// <summary>
        /// Modifies the current channel.
        /// </summary>
        /// <param name="name">New name for the channel.</param>
        /// <param name="position">New position for the channel.</param>
        /// <param name="topic">New topic for the channel.</param>
        /// <param name="parent">Category to put this channel in.</param>
        /// <param name="bitrate">New voice bitrate for the channel.</param>
        /// <param name="user_limit">New user limit for the channel.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task ModifyAsync(string name = null, int? position = null, string topic = null, Optional<DiscordChannel> parent = default(Optional<DiscordChannel>), int? bitrate = null, 
            int? user_limit = null, string reason = null)
        {
            return this.Discord.ApiClient.ModifyChannelAsync(this.Id, name, position, topic, parent.HasValue ? parent.Value?.Id : default(Optional<ulong?>), bitrate, user_limit, reason);
        }

        /// <summary>
        /// Updates the channel position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task ModifyPositionAsync(int position, string reason = null)
        {
            if (this.Guild == null)
                throw new InvalidOperationException("Cannot modify order of non-guild channels.");

            var chns = this.Guild._channels.Where(xc => xc.Type == this.Type).OrderBy(xc => xc.Position).ToArray();
            var pmds = new RestGuildChannelReorderPayload[chns.Length];
            for (var i = 0; i < chns.Length; i++)
            {
                pmds[i] = new RestGuildChannelReorderPayload
                {
                    ChannelId = chns[i].Id
                };

                if (chns[i].Id == this.Id)
                    pmds[i].Position = position;
                else
                    pmds[i].Position = chns[i].Position >= position ? chns[i].Position + 1 : chns[i].Position;
            }

            return this.Discord.ApiClient.ModifyGuildChannelPosition(this.Guild.Id, pmds, reason);
        }

        /// <summary>  
        /// Returns a list of messages. Only set ONE of the three parameters. They are Message ID's
        /// </summary> 
        public Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(int limit = 100, ulong? before = null, ulong? after = null, ulong? around = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("Cannot get the messages of a non-text channel");

            return this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, limit, before, after, around);
        }

        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteMessagesAsync(IEnumerable<DiscordMessage> messages, string reason = null)
        {
            // don't enumerate more than once
            var msgs = messages as DiscordMessage[] ?? messages.ToArray();
            if (messages == null || !msgs.Any())
                throw new ArgumentException("You need to specify at least one message to delete.");

            if (msgs.Count() < 2)
                return this.Discord.ApiClient.DeleteMessageAsync(this.Id, msgs.Single().Id, reason);
            return this.Discord.ApiClient.DeleteMessagesAsync(this.Id, msgs.Where(xm => xm.Channel.Id == this.Id).Select(xm => xm.Id), reason);
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteMessageAsync(DiscordMessage message, string reason = null)
        {
            return this.Discord.ApiClient.DeleteMessageAsync(this.Id, message.Id, reason);
        }

        /// <summary>
        /// Returns a list of invite objects
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
        {
            if (this.Guild == null)
                throw new ArgumentException("Cannot get the invites of a channel that does not belong to a Guild");

            return this.Discord.ApiClient.GetChannelInvitesAsync(Id);
        }

        /// <summary>
        /// Create a new invite object
        /// </summary>
        /// <param name="max_age"></param>
        /// <param name="max_uses"></param>
        /// <param name="temporary"></param>
        /// <param name="unique"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task<DiscordInvite> CreateInviteAsync(int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false, string reason = null)
        {
            return this.Discord.ApiClient.CreateChannelInviteAsync(Id, max_age, max_uses, temporary, unique, reason);
        }

        /// <summary>
        /// Deletes a channel permission overwrite
        /// </summary>
        /// <param name="overwrite"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteOverwriteAsync(DiscordOverwrite overwrite, string reason = null) =>
            this.Discord.ApiClient.DeleteChannelPermissionAsync(this.Id, overwrite.Id, reason);

        /// <summary>
        /// Updates a channel permission overwrite.
        /// </summary>
        /// <param name="overwrite">Overwrite to update.</param>
        /// <param name="allow">Permissions to allow.</param>
        /// <param name="deny">Permissions to deny.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task UpdateOverwriteAsync(DiscordOverwrite overwrite, Permissions allow, Permissions deny, string reason = null) =>
            this.Discord.ApiClient.EditChannelPermissionsAsync(this.Id, overwrite.Id, allow, deny, overwrite.Type, reason);

        /// <summary>
        /// Adds a channel permission overwrite for specified member.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task AddOverwriteAsync(DiscordMember member, Permissions allow, Permissions deny, string reason = null) =>
            this.Discord.ApiClient.EditChannelPermissionsAsync(this.Id, member.Id, allow, deny, "member", reason);

        /// <summary>
        /// Adds a channel permission overwrite for specified role.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task AddOverwriteAsync(DiscordRole role, Permissions allow, Permissions deny, string reason = null) =>
            this.Discord.ApiClient.EditChannelPermissionsAsync(this.Id, role.Id, allow, deny, "role", reason);

        /// <summary>
        /// Post a typing indicator
        /// </summary>
        /// <returns></returns>
        public Task TriggerTypingAsync()
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("Cannot start typing in a non-text channel");

            return this.Discord.ApiClient.TriggerTypingAsync(Id);
        }

        /// <summary>
        /// Returns all pinned messages
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync()
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group)
                throw new ArgumentException("A non-text channel does not have pinned messages");

            return this.Discord.ApiClient.GetPinnedMessagesAsync(this.Id);
        }

        /// <summary>
        /// Create a new webhook
        /// </summary>
        /// <param name="name"></param>
        /// <param name="avatar"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public async Task<DiscordWebhook> CreateWebhookAsync(string name, Stream avatar = null, string reason = null)
        {
            string av64 = null;
            if (avatar != null)
                using (var imgtool = new ImageTool(avatar))
                    av64 = imgtool.GetBase64();

            return await this.Discord.ApiClient.CreateWebhookAsync(this.Id, name, av64, reason);
        }

        /// <summary>
        /// Returns a list of webhooks
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync() =>
            this.Discord.ApiClient.GetChannelWebhooksAsync(this.Id);

        /// <summary>
        /// Moves a member to this voice channel
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public async Task PlaceMemberAsync(DiscordMember member)
        {
            if (this.Type != ChannelType.Voice)
                throw new ArgumentException("Cannot place member in a non-voice channel");
            
            await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, member.Id, null, null, null, null,
                Id, null);
        }

        /// <summary>
        /// Calculates permissions for a given member.
        /// </summary>
        /// <param name="mbr">Member to calculate permissions for.</param>
        /// <returns>Calculated permissions for a given member.</returns>
        public Permissions PermissionsFor(DiscordMember mbr)
        {
            // default permissions
            const Permissions def = Permissions.None;
            
            // future note: might be able to simplify @everyone role checks to just check any role ... but i'm not sure
            // xoxo, ~uwx
            //
            // you should use a single tilde
            // ~emzi
            
            // user > role > everyone
            // allow > deny > undefined
            // =>
            // user allow > user deny > role allow > role deny > everyone allow > everyone deny
            // thanks to meew0

            if (this.IsPrivate || this.Guild == null)
                return def;

            Permissions perms;

            // assign @everyone permissions
            var everyoneRole = this.Guild.EveryoneRole;
            perms = everyoneRole.Permissions;

            // roles that member is in
            var mbRoles = mbr.Roles.Where(xr => xr.Id != everyoneRole.Id).ToArray();
            // channel overrides for roles that member is in
            var mbRoleOverrides = mbRoles
                .Select(xr => this._permission_overwrites.FirstOrDefault(xo => xo.Id == xr.Id))
                .Where(xo => xo != null)
                .ToList();

            // assign permissions from member's roles (in order)
            perms |= mbRoles.Aggregate(def, (c, role) => c | role.Permissions);
            
            // assign channel permission overwrites for @everyone pseudo-role
            var everyoneOverwrites = this._permission_overwrites.FirstOrDefault(xo => xo.Id == everyoneRole.Id);
            if (everyoneOverwrites != null)
            {
                perms &= ~everyoneOverwrites.Deny;
                perms |= everyoneOverwrites.Allow;
            }

            // assign channel permission overwrites for member's roles (explicit deny)
            perms &= ~mbRoleOverrides.Aggregate(def, (c, overs) => c | overs.Deny);
            // assign channel permission overwrites for member's roles (explicit allow)
            perms |= mbRoleOverrides.Aggregate(def, (c, overs) => c | overs.Allow);

            // channel overrides for just this member
            var mbOverrides = this._permission_overwrites.FirstOrDefault(xo => xo.Id == mbr.Id);
            if (mbOverrides == null) return perms;
            
            // assign channel permission overwrites for just this member
            perms &= ~mbOverrides.Deny;
            perms |= mbOverrides.Allow;

            return perms;
        }

        /// <summary>
        /// Returns a string representation of this channel.
        /// </summary>
        /// <returns>String representation of this channel.</returns>
        public override string ToString()
        {
            if (this.Type == ChannelType.Category)
                return string.Concat("Channel Category ", this.Name, " (", this.Id, ")");
            if (this.Type == ChannelType.Text)
                return string.Concat("Channel #", this.Name, " (", this.Id, ")");
            if (!string.IsNullOrWhiteSpace(this.Name))
                return string.Concat("Channel ", this.Name, " (", this.Id, ")");
            return string.Concat("Channel ", this.Id);
        }
        #endregion

        /// <summary>
        /// Checks whether this <see cref="DiscordChannel"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordChannel"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordChannel);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordChannel"/> is equal to another <see cref="DiscordChannel"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordChannel"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordChannel"/> is equal to this <see cref="DiscordChannel"/>.</returns>
        public bool Equals(DiscordChannel e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordChannel"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordChannel"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordChannel"/> objects are equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are equal.</returns>
        public static bool operator ==(DiscordChannel e1, DiscordChannel e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordChannel"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First channel to compare.</param>
        /// <param name="e2">Second channel to compare.</param>
        /// <returns>Whether the two channels are not equal.</returns>
        public static bool operator !=(DiscordChannel e1, DiscordChannel e2) =>
            !(e1 == e2);
    }
}
