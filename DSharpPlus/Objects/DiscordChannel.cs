using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
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
        
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        internal string _type;

        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        [JsonIgnore]
        public ChannelType Type
        {
            get
            {
                switch (this._type.ToLower())
                {
                    case "text":
                        return ChannelType.Text;

                    case "voice":
                        return ChannelType.Voice;
                }

                return ChannelType.Unknown;
            }
        }

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
            this.Discord._rest_client.InternalCreateMessageAsync(Id, content, tts, embed);

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
            this.Discord._rest_client.InternalUploadFileAsync(Id, file_data, file_name, content, tts, embed);
        /// <summary>
        /// Posts a file
        /// </summary>
        /// <param name="files"></param>
        /// <param name="content"></param>
        /// <param name="tts"></param>
        /// <param name="embed"></param>
        /// <returns></returns>
        public Task<DiscordMessage> SendMultipleFilesAsync(Dictionary<string, Stream> files, string content = "", bool tts = false, DiscordEmbed embed = null) =>
            this.Discord._rest_client.InternalUploadFilesAsync(Id, files, content, tts, embed);

        // Please send memes to Naamloos#2887 at discord <3 thank you

        /// <summary>
        /// Deletes a guild channel
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) =>
            this.Discord._rest_client.InternalDeleteChannelAsync(Id, reason);

        /// <summary>
        /// Returns a specific message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DiscordMessage> GetMessageAsync(ulong id)
        {
            if (this.Discord._config.MessageCacheSize > 0 && this.MessageCache.TryGet(xm => xm.Id == id, out var msg))
                return msg;

            return await this.Discord._rest_client.InternalGetMessageAsync(Id, id);
        }

        /// <summary>
        /// Modifies the current channel.
        /// </summary>
        /// <param name="name">New name for the channel.</param>
        /// <param name="position">New position for the channel.</param>
        /// <param name="topic">New topic for the channel.</param>
        /// <param name="bitrate">New voice bitrate for the channel.</param>
        /// <param name="user_limit">New user limit for the channel.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task ModifyAsync(string name = null, int? position = null, string topic = null, int? bitrate = null, int? user_limit = null, string reason = null) =>
            this.Discord._rest_client.InternalModifyChannelAsync(this.Id, name, position, topic, bitrate, user_limit, reason);

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

            return this.Discord._rest_client.InternalModifyGuildChannelPosition(this.Guild.Id, pmds, reason);
        }

        /// <summary>  
        /// Returns a list of messages.Only set ONE of the three parameters. They are Message ID's
        /// </summary> 
        public Task<IReadOnlyCollection<DiscordMessage>> GetMessagesAsync(int limit = 100, ulong? before = null, ulong? after = null, ulong? around = null) =>
            this.Discord._rest_client.InternalGetChannelMessagesAsync(this.Id, limit, before, after, around);

        /// <summary>
        /// Deletes multiple messages
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteMessagesAsync(IEnumerable<DiscordMessage> messages, string reason = null) =>
            this.Discord._rest_client.InternalDeleteMessagesAsync(this.Id, messages.Where(xm => xm.Channel.Id == this.Id).Select(xm => xm.Id), reason);

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteMessageAsync(DiscordMessage message, string reason = null) =>
            this.Discord._rest_client.InternalDeleteMessageAsync(this.Id, message.Id, reason);

        /// <summary>
        /// Returns a list of invite objects
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyCollection<DiscordInvite>> GetInvitesAsync() =>
            this.Discord._rest_client.InternalGetChannelInvitesAsync(Id);

        /// <summary>
        /// Create a new invite object
        /// </summary>
        /// <param name="max_age"></param>
        /// <param name="max_uses"></param>
        /// <param name="temporary"></param>
        /// <param name="unique"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task<DiscordInvite> CreateInviteAsync(int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false, string reason = null) => 
            this.Discord._rest_client.InternalCreateChannelInviteAsync(Id, max_age, max_uses, temporary, unique, reason);

        /// <summary>
        /// Deletes a channel permission overwrite
        /// </summary>
        /// <param name="overwrite"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task DeleteOverwriteAsync(DiscordOverwrite overwrite, string reason = null) =>
            this.Discord._rest_client.InternalDeleteChannelPermissionAsync(this.Id, overwrite.Id, reason);

        /// <summary>
        /// Updates a channel permission overwrite
        /// </summary>
        /// <param name="overwrite"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task UpdateOverwriteAsync(DiscordOverwrite overwrite, string reason = null) =>
            this.Discord._rest_client.InternalEditChannelPermissionsAsync(this.Id, overwrite.Id, overwrite.Allow, overwrite.Deny, overwrite.Type, reason);

        /// <summary>
        /// Adds a channel permission overwrite for specified member.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task AddOverwriteAsync(DiscordMember member, Permissions allow, Permissions deny, string reason = null) =>
            this.Discord._rest_client.InternalEditChannelPermissionsAsync(this.Id, member.Id, allow, deny, "member", reason);

        /// <summary>
        /// Adds a channel permission overwrite for specified role.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="allow"></param>
        /// <param name="deny"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task AddOverwriteAsync(DiscordRole role, Permissions allow, Permissions deny, string reason = null) =>
            this.Discord._rest_client.InternalEditChannelPermissionsAsync(this.Id, role.Id, allow, deny, "role", reason);

        /// <summary>
        /// Post a typing indicator
        /// </summary>
        /// <returns></returns>
        public Task TriggerTypingAsync() =>
            this.Discord._rest_client.InternalTriggerTypingAsync(Id);

        /// <summary>
        /// Returns all pinned messages
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyCollection<DiscordMessage>> GetPinnedMessagesAsync() =>
            this.Discord._rest_client.InternalGetPinnedMessagesAsync(this.Id);

        /// <summary>
        /// Create a new webhook
        /// </summary>
        /// <param name="name"></param>
        /// <param name="avatar"></param>
        /// <param name="avatar_format"></param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public async Task<DiscordWebhook> CreateWebhookAsync(string name, Stream avatar = null, ImageFormat? avatar_format = null, string reason = null)
        {
            string av64 = null;
            if (avatar != null)
            {
                if (avatar_format == null)
                    throw new ArgumentNullException("When specifying new avatar, you must specify its format.");

                using (var ms = new MemoryStream((int)(avatar.Length - avatar.Position)))
                {
                    await avatar.CopyToAsync(ms);
                    av64 = string.Concat("data:image/", avatar_format.Value.ToString().ToLower(), ";base64,", Convert.ToBase64String(ms.ToArray()));
                }
            }

            return await this.Discord._rest_client.InternalCreateWebhookAsync(this.Id, name, av64, reason);
        }

        /// <summary>
        /// Returns a list of webhooks
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyCollection<DiscordWebhook>> GetWebhooksAsync() =>
            this.Discord._rest_client.InternalGetChannelWebhooksAsync(this.Id);

        /// <summary>
        /// Moves a member to this voice channel
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public async Task PlaceMemberAsync(DiscordMember member)
        {
            if (Type == ChannelType.Voice)
                await this.Discord._rest_client.InternalModifyGuildMemberAsync(this.Guild.Id, member.Id, null, null, null, null, Id, null);
        }

        public Permissions PermissionsFor(DiscordMember mbr)
        {
            // user > role > everyone
            // allow > deny > undefined
            // =>
            // user allow > user deny > role allow > role deny > everyone allow > everyone deny
            // thanks to meew0

            if (this.IsPrivate || this.Guild == null)
                return Permissions.None;

            var prms = Permissions.None;

            var ev1 = this.Guild.EveryoneRole;
            var evo = this._permission_overwrites.FirstOrDefault(xo => xo.Id == ev1.Id);

            prms = ev1.Permissions;
            if (evo != null)
            {
                prms &= ~evo.Deny;
                prms |= evo.Allow;
            }

            var rls = mbr.Roles.Where(xr => xr.Id != ev1.Id);
            var rlo = rls.Select(xr => this._permission_overwrites.FirstOrDefault(xo => xo.Id == xr.Id)).Where(xo => xo != null);

            var rdeny = Permissions.None;
            var rallw = Permissions.None;
            foreach (var xrl in rls)
            {
                rallw |= xrl.Permissions;
            }
            foreach (var xpo in rlo)
            {
                rdeny |= xpo.Deny;
                rallw |= xpo.Allow;
            }

            prms &= ~rdeny;
            prms |= rallw;

            var rmo = this._permission_overwrites.FirstOrDefault(xo => xo.Id == mbr.Id);
            if (rmo != null)
            {
                prms &= ~rmo.Deny;
                prms |= rmo.Allow;
            }

            return prms;
        }
        #endregion

    }
}
