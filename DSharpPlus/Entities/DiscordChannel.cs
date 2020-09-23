using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
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
        public DiscordChannel Parent 
            => this.ParentId.HasValue ? this.Guild.GetChannel(this.ParentId.Value) : null;

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
        public int Position { get; internal set; }

        /// <summary>
        /// Gets whether this channel is a DM channel.
        /// </summary>
        [JsonIgnore]
        public bool IsPrivate 
            => this.Type == ChannelType.Private || this.Type == ChannelType.Group;

        /// <summary>
        /// Gets whether this channel is a channel category.
        /// </summary>
        [JsonIgnore]
        public bool IsCategory 
            => this.Type == ChannelType.Category;

        /// <summary>
        /// Gets the guild to which this channel belongs.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild 
            => this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;

        /// <summary>
        /// Gets a collection of permission overwrites for this channel.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordOverwrite> PermissionOverwrites 
            => this._permissionOverwritesLazy.Value;

        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordOverwrite> _permissionOverwrites = new List<DiscordOverwrite>();
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordOverwrite>> _permissionOverwritesLazy;

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
        /// <para>Gets the slow mode delay configured for this channel.</para>
        /// <para>All bots, as well as users with <see cref="Permissions.ManageChannels"/> or <see cref="Permissions.ManageMessages"/> permissions in the channel are exempt from slow mode.</para>
        /// </summary>
        [JsonProperty("rate_limit_per_user")]
        public int? PerUserRateLimit { get; internal set; }

        /// <summary>
        /// Gets this channel's mention string.
        /// </summary>
        [JsonIgnore]
        public string Mention 
            => Formatter.Mention(this);
        
        /// <summary>
        /// Gets this channel's children. This applies only to channel categories.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordChannel> Children
        {
            get
            {
                if (!IsCategory)
                    throw new ArgumentException("Only channel categories contain children.");

                return Guild._channels.Values.Where(e => e.ParentId == Id);
            }
        }

        /// <summary>
        /// Gets the list of members currently in the channel (if voice channel), or members who can see the channel (otherwise).
        /// </summary>
        [JsonIgnore]
        public virtual IEnumerable<DiscordMember> Users
        {
            get
            {
                if (this.Guild == null)
                    throw new InvalidOperationException("Cannot query users outside of guild channels.");

                if (this.Type == ChannelType.Voice)
                    return Guild.Members.Values.Where(x => x.VoiceState?.ChannelId == this.Id);

                return Guild.Members.Values.Where(x => (this.PermissionsFor(x) & Permissions.AccessChannels) == Permissions.AccessChannels);
            }
        }

        /// <summary>
        /// Gets whether this channel is an NSFW channel.
        /// </summary>
        [JsonProperty("nsfw")]
        public bool IsNSFW { get; internal set; }

        internal DiscordChannel()
        {
            this._permissionOverwritesLazy = new Lazy<IReadOnlyList<DiscordOverwrite>>(() => new ReadOnlyCollection<DiscordOverwrite>(this._permissionOverwrites));
        }

        #region Methods
        /// <summary>
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <param name="mentions">Allowed mentions in the message</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendMessageAsync(string content = null, bool tts = false, DiscordEmbed embed = null, IEnumerable<IMention> mentions = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("Cannot send a text message to a non-text channel.");
            
            return this.Discord.ApiClient.CreateMessageAsync(this.Id, content, tts, embed, mentions);
        }

        /// <summary>
        /// Sends a message containing an attached file to this channel.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach to the message as a file.</param>
        /// <param name="fileName">Name of the file to attach to the message.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <param name="mentions">Allowed mentions in the message</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendFileAsync(string fileName, Stream fileData, string content = null, bool tts = false, DiscordEmbed embed = null, IEnumerable<IMention> mentions = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("Cannot send a file to a non-text channel.");

            return this.Discord.ApiClient.UploadFileAsync(this.Id, fileData, fileName, content, tts, embed, mentions);
        }

        /// <summary>
        /// Sends a message containing an attached file to this channel.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach to the message as a file.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <param name="mentions">Allowed mentions in the message</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendFileAsync(FileStream fileData, string content = null, bool tts = false, DiscordEmbed embed = null, IEnumerable<IMention> mentions = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("Cannot send a file to a non-text channel.");
            
            return this.Discord.ApiClient.UploadFileAsync(this.Id, fileData, Path.GetFileName(fileData.Name), content,
                tts, embed, mentions);
        }

        /// <summary>
        /// Sends a message containing an attached file to this channel.
        /// </summary>
        /// <param name="filePath">Path to the file to be attached to the message.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <param name="mentions">Allowed mentions in the message</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(string filePath, string content = null, bool tts = false, DiscordEmbed embed = null, IEnumerable<IMention> mentions = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("Cannot send a file to a non-text channel.");

            using (var fs = File.OpenRead(filePath))
                return await this.Discord.ApiClient.UploadFileAsync(this.Id, fs, Path.GetFileName(fs.Name), content, tts, embed, mentions).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a message with several attached files to this channel.
        /// </summary>
        /// <param name="files">A filename to data stream mapping.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <param name="mentions">Allowed mentions in the message</param>
        /// <returns>The sent message.</returns>
        public Task<DiscordMessage> SendMultipleFilesAsync(Dictionary<string, Stream> files, string content = "", bool tts = false, DiscordEmbed embed = null, IEnumerable<IMention> mentions = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("Cannot send a file to a non-text channel.");

            if (files.Count > 10)
                throw new ArgumentException("Cannot send more than 10 files with a single message.");

            return this.Discord.ApiClient.UploadFilesAsync(this.Id, files, content, tts, embed, mentions);
        }

        // Please send memes to Naamloos#2887 at discord <3 thank you

        /// <summary>
        /// Deletes a guild channel
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) 
            => this.Discord.ApiClient.DeleteChannelAsync(Id, reason);

        /// <summary>
        /// Clones this channel. This operation will create a channel with identical settings to this one. Note that this will not copy messages.
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns>Newly-created channel.</returns>
        public async Task<DiscordChannel> CloneAsync(string reason = null)
        {
            if (this.Guild == null)
                throw new InvalidOperationException("Non-guild channels cannot be cloned.");

            var ovrs = new List<DiscordOverwriteBuilder>();
            foreach (var ovr in this._permissionOverwrites)
                ovrs.Add(await new DiscordOverwriteBuilder().FromAsync(ovr).ConfigureAwait(false));

            int? bitrate = this.Bitrate;
            int? userLimit = this.UserLimit;
            Optional<int?> perUserRateLimit = this.PerUserRateLimit;

            if (this.Type != ChannelType.Voice)
            {
                bitrate = null;
                userLimit = null;
            }
            if (this.Type != ChannelType.Text)
            {
                perUserRateLimit = Optional.FromNoValue<int?>();
            }

            return await this.Guild.CreateChannelAsync(this.Name, this.Type, this.Parent, this.Topic, bitrate, userLimit, ovrs, this.IsNSFW, perUserRateLimit, reason).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a specific message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessageAsync(ulong id)
        {
            if (this.Discord.Configuration.MessageCacheSize > 0 
                && this.Discord is DiscordClient dc 
                && dc.MessageCache != null 
                && dc.MessageCache.TryGet(xm => xm.Id == id && xm.ChannelId == this.Id, out var msg))
            {
                return msg;
            }

            return await this.Discord.ApiClient.GetMessageAsync(Id, id).ConfigureAwait(false);
        }

        /// <summary>
        /// Modifies the current channel.
        /// </summary>
        /// <param name="action">Action to perform on this channel</param>
        /// <returns></returns>
        public Task ModifyAsync(Action<ChannelEditModel> action)
        {
            var mdl = new ChannelEditModel();
            action(mdl);
            return this.Discord.ApiClient.ModifyChannelAsync(this.Id, mdl.Name, mdl.Position, mdl.Topic, mdl.Nsfw,
                mdl.Parent.HasValue ? mdl.Parent.Value?.Id : default(Optional<ulong?>), mdl.Bitrate, mdl.Userlimit, mdl.PerUserRateLimit, 
                mdl.AuditLogReason);
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

            var chns = this.Guild._channels.Values.Where(xc => xc.Type == this.Type).OrderBy(xc => xc.Position).ToArray();
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

            return this.Discord.ApiClient.ModifyGuildChannelPositionAsync(this.Guild.Id, pmds, reason);
        }

        /// <summary>  
        /// Returns a list of messages before a certain message.
        /// <param name="limit">The amount of messages to fetch.</param>
        /// <param name="before">Message to fetch before from.</param>
        /// </summary> 
        public Task<IReadOnlyList<DiscordMessage>> GetMessagesBeforeAsync(ulong before, int limit = 100)
            => this.GetMessagesInternalAsync(limit, before, null, null);
        
        /// <summary>  
        /// Returns a list of messages after a certain message.
        /// <param name="limit">The amount of messages to fetch.</param>
        /// <param name="after">Message to fetch after from.</param>
        /// </summary> 
        public Task<IReadOnlyList<DiscordMessage>> GetMessagesAfterAsync(ulong after, int limit = 100)
            => this.GetMessagesInternalAsync(limit, null, after, null);
        
        /// <summary>  
        /// Returns a list of messages around a certain message.
        /// <param name="limit">The amount of messages to fetch.</param>
        /// <param name="around">Message to fetch around from.</param>
        /// </summary> 
        public Task<IReadOnlyList<DiscordMessage>> GetMessagesAroundAsync(ulong around, int limit = 100)
            => this.GetMessagesInternalAsync(limit, null, null, around);

        /// <summary>  
        /// Returns a list of messages from the last message in the channel.
        /// <param name="limit">The amount of messages to fetch.</param>
        /// </summary> 
        public Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(int limit = 100) =>
            this.GetMessagesInternalAsync(limit, null, null, null);

        private async Task<IReadOnlyList<DiscordMessage>> GetMessagesInternalAsync(int limit = 100, ulong? before = null, ulong? after = null, ulong? around = null)
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("Cannot get the messages of a non-text channel.");

            if (limit < 0)
                throw new ArgumentException("Cannot get a negative number of messages.");

            if (limit == 0)
                return new DiscordMessage[0];

            //return this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, limit, before, after, around);
            if (limit > 100 && around != null)
                throw new InvalidOperationException("Cannot get more than 100 messages around the specified ID.");

            var msgs = new List<DiscordMessage>(limit);
            var remaining = limit;
            var lastCount = 0;
            ulong? last = null;
            var isAfter = after != null;

            do
            {
                var fetchSize = remaining > 100 ? 100 : remaining;
                var fetch = await this.Discord.ApiClient.GetChannelMessagesAsync(this.Id, fetchSize, !isAfter ? last ?? before : null, isAfter ? last ?? after : null, around).ConfigureAwait(false);

                lastCount = fetch.Count;
                remaining -= lastCount;

                if (!isAfter)
                {
                    msgs.AddRange(fetch);
                    last = fetch.LastOrDefault()?.Id;
                }
                else
                {
                    msgs.InsertRange(0, fetch);
                    last = fetch.FirstOrDefault()?.Id;
                }
            }
            while (remaining > 0 && lastCount > 0);

            return new ReadOnlyCollection<DiscordMessage>(msgs);
        }

        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public async Task DeleteMessagesAsync(IEnumerable<DiscordMessage> messages, string reason = null)
        {
            // don't enumerate more than once
            var msgs = messages.Where(x => x.Channel.Id == this.Id).Select(x => x.Id).ToArray();
            if (messages == null || !msgs.Any())
                throw new ArgumentException("You need to specify at least one message to delete.");

            if (msgs.Count() < 2)
            {
                await this.Discord.ApiClient.DeleteMessageAsync(this.Id, msgs.Single(), reason).ConfigureAwait(false);
                return;
            }
            
            for (var i = 0; i < msgs.Count(); i += 100)
                await this.Discord.ApiClient.DeleteMessagesAsync(this.Id, msgs.Skip(i).Take(100), reason).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteMessageAsync(DiscordMessage message, string reason = null)
            => this.Discord.ApiClient.DeleteMessageAsync(this.Id, message.Id, reason);

        /// <summary>
        /// Returns a list of invite objects
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
        {
            if (this.Guild == null)
                throw new ArgumentException("Cannot get the invites of a channel that does not belong to a guild.");

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
            => this.Discord.ApiClient.CreateChannelInviteAsync(Id, max_age, max_uses, temporary, unique, reason);

        /// <summary>
        /// Adds a channel permission overwrite for specified member.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task AddOverwriteAsync(DiscordMember member, Permissions allow = Permissions.None, Permissions deny = Permissions.None, string reason = null) 
            => this.Discord.ApiClient.EditChannelPermissionsAsync(this.Id, member.Id, allow, deny, "member", reason);

        /// <summary>
        /// Adds a channel permission overwrite for specified role.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task AddOverwriteAsync(DiscordRole role, Permissions allow = Permissions.None, Permissions deny = Permissions.None, string reason = null) 
            => this.Discord.ApiClient.EditChannelPermissionsAsync(this.Id, role.Id, allow, deny, "role", reason);

        /// <summary>
        /// Post a typing indicator
        /// </summary>
        /// <returns></returns>
        public Task TriggerTypingAsync()
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("Cannot start typing in a non-text channel.");

            return this.Discord.ApiClient.TriggerTypingAsync(Id);
        }

        /// <summary>
        /// Returns all pinned messages
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync()
        {
            if (this.Type != ChannelType.Text && this.Type != ChannelType.Private && this.Type != ChannelType.Group && this.Type != ChannelType.News)
                throw new ArgumentException("A non-text channel does not have pinned messages.");

            return this.Discord.ApiClient.GetPinnedMessagesAsync(this.Id);
        }

        /// <summary>
        /// Create a new webhook
        /// </summary>
        /// <param name="name"></param>
        /// <param name="avatar"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public async Task<DiscordWebhook> CreateWebhookAsync(string name, Optional<Stream> avatar = default, string reason = null)
        {
            var av64 = Optional.FromNoValue<string>();
            if (avatar.HasValue && avatar.Value != null)
                using (var imgtool = new ImageTool(avatar.Value))
                    av64 = imgtool.GetBase64();
            else if (avatar.HasValue)
                av64 = null;

            return await this.Discord.ApiClient.CreateWebhookAsync(this.Id, name, av64, reason).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a list of webhooks
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync() 
            => this.Discord.ApiClient.GetChannelWebhooksAsync(this.Id);

        /// <summary>
        /// Moves a member to this voice channel
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public async Task PlaceMemberAsync(DiscordMember member)
        {
            if (this.Type != ChannelType.Voice)
                throw new ArgumentException("Cannot place a member in a non-voice channel!"); // be a little more angery, let em learn!!1
            
            await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, member.Id, default, default, default,
                default, this.Id, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Follows a news channel
        /// </summary>
        /// <param name="targetChannel">Channel to crosspost messages to</param>
        /// <exception cref="ArgumentException">Thrown when trying to follow a non-news channel</exception>
        /// <exception cref="UnauthorizedException">Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> on the target channel</exception>
        public Task<DiscordFollowedChannel> FollowAsync(DiscordChannel targetChannel)
        {
            if (this.Type != ChannelType.News)
                throw new ArgumentException("Cannot follow a non-news channel.");

            return this.Discord.ApiClient.FollowChannelAsync(this.Id, targetChannel.Id);
        }

        /// <summary>
        /// Publishes a message in a news channel to following channels
        /// </summary>
        /// <param name="message">Message to publish</param>
        /// <exception cref="ArgumentException">Thrown when the message has already been crossposted</exception>
        /// <exception cref="UnauthorizedException">
        ///     Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> and/or <see cref="Permissions.SendMessages"/> 
        /// </exception>
        public Task<DiscordMessage> CrosspostMessageAsync(DiscordMessage message)
        {
            if ((message.Flags & MessageFlags.Crossposted) == MessageFlags.Crossposted)
                throw new ArgumentException("Message is already crossposted.");
            
            return this.Discord.ApiClient.CrosspostMessageAsync(this.Id, message.Id);
        }

        /// <summary>
        /// Calculates permissions for a given member.
        /// </summary>
        /// <param name="mbr">Member to calculate permissions for.</param>
        /// <returns>Calculated permissions for a given member.</returns>
        public Permissions PermissionsFor(DiscordMember mbr)
        {
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
                return Permissions.None;

            if (this.Guild.OwnerId == mbr.Id)
                return PermissionMethods.FULL_PERMS;

            Permissions perms;

            // assign @everyone permissions
            var everyoneRole = this.Guild.EveryoneRole;
            perms = everyoneRole.Permissions;

            // roles that member is in
            var mbRoles = mbr.Roles.Where(xr => xr.Id != everyoneRole.Id).ToArray();

            // assign permissions from member's roles (in order)
            perms |= mbRoles.Aggregate(Permissions.None, (c, role) => c | role.Permissions);

            // Adminstrator grants all permissions and cannot be overridden
            if ((perms & Permissions.Administrator) == Permissions.Administrator)
                return PermissionMethods.FULL_PERMS;

            // channel overrides for roles that member is in
            var mbRoleOverrides = mbRoles
                .Select(xr => this._permissionOverwrites.FirstOrDefault(xo => xo.Id == xr.Id))
                .Where(xo => xo != null)
                .ToList();
            
            // assign channel permission overwrites for @everyone pseudo-role
            var everyoneOverwrites = this._permissionOverwrites.FirstOrDefault(xo => xo.Id == everyoneRole.Id);
            if (everyoneOverwrites != null)
            {
                perms &= ~everyoneOverwrites.Denied;
                perms |= everyoneOverwrites.Allowed;
            }

            // assign channel permission overwrites for member's roles (explicit deny)
            perms &= ~mbRoleOverrides.Aggregate(Permissions.None, (c, overs) => c | overs.Denied);
            // assign channel permission overwrites for member's roles (explicit allow)
            perms |= mbRoleOverrides.Aggregate(Permissions.None, (c, overs) => c | overs.Allowed);

            // channel overrides for just this member
            var mbOverrides = this._permissionOverwrites.FirstOrDefault(xo => xo.Id == mbr.Id);
            if (mbOverrides == null) return perms;
            
            // assign channel permission overwrites for just this member
            perms &= ~mbOverrides.Denied;
            perms |= mbOverrides.Allowed;

            return perms;
        }

        /// <summary>
        /// Returns a string representation of this channel.
        /// </summary>
        /// <returns>String representation of this channel.</returns>
        public override string ToString()
        {
            if (this.Type == ChannelType.Category)
                return $"Channel Category {this.Name} ({this.Id})";
            if (this.Type == ChannelType.Text || this.Type == ChannelType.News)
                return $"Channel #{this.Name} ({this.Id})";
            if (!string.IsNullOrWhiteSpace(this.Name))
                return $"Channel {this.Name} ({this.Id})";
            return $"Channel {this.Id}";
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
        public static bool operator !=(DiscordChannel e1, DiscordChannel e2) 
            => !(e1 == e2);
    }
}
