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
    /// Represents a Discord guild.
    /// </summary>
    public class DiscordGuild : SnowflakeObject
    {
        /// <summary>
        /// Gets the guild's name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the guild icon's hash.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconHash { get; internal set; }

        /// <summary>
        /// Gets the guild icon's url.
        /// </summary>
        [JsonIgnore]
        public string IconUrl => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/icons/{Id}/{IconHash}.jpg" : null;

        /// <summary>
        /// Gets the guild splash's hash.
        /// </summary>
        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string SplashHash { get; internal set; }

        /// <summary>
        /// Gets the ID of the guild's owner.
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong OwnerId { get; set; }

        /// <summary>
        /// Gets the guild's owner.
        /// </summary>
        [JsonIgnore]
        public DiscordMember Owner => this.Members.FirstOrDefault(xm => xm.Id == this.OwnerId) ?? this.Discord._rest_client.InternalGetGuildMemberAsync(this.Id, this.OwnerId).GetAwaiter().GetResult();

        /// <summary>
        /// Gets the guild's voice region ID.
        /// </summary>
        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string RegionId { get; internal set; }

        /// <summary>
        /// Gets the guild's AFK voice channel ID.
        /// </summary>
        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong AfkChannelId { get; set; } = 0;

        /// <summary>
        /// Gets the guild's AFK voice channel.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel AfkChannel => this.Channels.FirstOrDefault(xc => xc.Id == this.AfkChannelId);

        /// <summary>
        /// Gets the guild's AFK timeout.
        /// </summary>
        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int AfkTimeout { get; internal set; }

        /// <summary>
        /// Gets whether this guild has the guild embed enabled.
        /// </summary>
        [JsonProperty("embed_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool EmbedEnabled { get; internal set; }

        /// <summary>
        /// Gets the ID of the channel from the guild's embed.
        /// </summary>
        [JsonProperty("embed_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong EmbedChannelId { get; set; }

        /// <summary>
        /// Gets the channel from the guild's embed.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel EmbedChannel => this.Channels.FirstOrDefault(xc => xc.Id == this.EmbedChannelId);

        /// <summary>
        /// Gets the guild's verification level.
        /// </summary>
        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public VerificationLevel VerificationLevel { get; internal set; }

        /// <summary>
        /// Gets the guild's default notification settings.
        /// </summary>
        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultMessageNotifications DefaultMessageNotifications { get; internal set; }

        /// <summary>
        /// Gets a collection of this guild's roles.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordRole> Roles => this._roles_lazy.Value;
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordRole> _roles;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordRole>> _roles_lazy;

        /// <summary>
        /// Gets a collection of this guild's emojis.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordEmoji> Emojis => this._emojis_lazy.Value;
        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordEmoji> _emojis;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordEmoji>> _emojis_lazy;

        /// <summary>
        /// Gets a collection of this guild's features.
        /// </summary>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> Features { get; internal set; }

        /// <summary>
        /// Gets the required multi-factor authentication level for this guild.
        /// </summary>
        [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
        public MfaLevel MfaLevel { get; internal set; }

        /// <summary>
        /// Gets this guild's join date.
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime JoinedAt { get; internal set; }

        /// <summary>
        /// Gets whether this guild is considered to be a large guild.
        /// </summary>
        [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
        public bool Large { get; internal set; }

        /// <summary>
        /// Gets whether this guild is unavailable.
        /// </summary>
        [JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
        public bool Unavailable { get; internal set; }

        /// <summary>
        /// Gets the total number of members in this guild.
        /// </summary>
        [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int MemberCount { get; internal set; }

        /// <summary>
        /// Gets a collection of all the voice states for this guilds.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordVoiceState> VoiceStates => this._voice_states_lazy.Value;
        [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordVoiceState> _voice_states;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordVoiceState>> _voice_states_lazy;

        /// <summary>
        /// Gets a collection of all the members that belong to this guild.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordMember> Members => this._members_lazy.Value;
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordMember> _members;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordMember>> _members_lazy;

        /// <summary>
        /// Gets a collection of all the channels associated with this guild.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordChannel> Channels => this._channels_lazy.Value;
        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordChannel> _channels;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordChannel>> _channels_lazy;

        /// <summary>
        /// Gets the default channel for this guild.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel DefaultChannel => this.Channels.FirstOrDefault(xc => xc.Id == this.Id);

        /// <summary>
        /// Gets a collection of all presences in this guild.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<DiscordPresence> Presences => this._presences_lazy.Value;
        [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
        internal List<DiscordPresence> _presences;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordPresence>> _presences_lazy;

        /// <summary>
        /// Gets whether the current user is the guild's owner.
        /// </summary>
        [JsonProperty("is_owner", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOwner => this.OwnerId == this.Discord.CurrentUser.Id;

        public DiscordGuild()
        {
            this._roles_lazy = new Lazy<IReadOnlyList<DiscordRole>>(() => new ReadOnlyCollection<DiscordRole>(this._roles));
            this._emojis_lazy = new Lazy<IReadOnlyList<DiscordEmoji>>(() => new ReadOnlyCollection<DiscordEmoji>(this._emojis));
            this._voice_states_lazy = new Lazy<IReadOnlyList<DiscordVoiceState>>(() => new ReadOnlyCollection<DiscordVoiceState>(this._voice_states));
            this._channels_lazy = new Lazy<IReadOnlyList<DiscordChannel>>(() => new ReadOnlyCollection<DiscordChannel>(this._channels));
            this._members_lazy = new Lazy<IReadOnlyList<DiscordMember>>(() => new ReadOnlyCollection<DiscordMember>(this._members));
            this._presences_lazy = new Lazy<IReadOnlyList<DiscordPresence>>(() => new ReadOnlyCollection<DiscordPresence>(this._presences));
        }

        #region Guild Methods
        public Task DeleteAsync() =>
            this.Discord._rest_client.InternalDeleteGuildAsync(this.Id);

        public async Task<DiscordGuild> ModifyAsync(string name = null, string region = null, Stream icon = null, VerificationLevel? verification_level = null, DefaultMessageNotifications? default_message_notifications = null, ulong? afk_channel_id = null, int? afk_timeout = null, ulong? owner_id = null, Stream splash = null, string reason = null)
        {
            string iconb64 = null;
            if (icon != null)
            {
                using (var ms = new MemoryStream((int)(icon.Length - icon.Position)))
                {
                    await icon.CopyToAsync(ms);
                    iconb64 = Convert.ToBase64String(ms.ToArray());
                }
            }

            string splashb64 = null;
            if (splash != null)
            {
                using (var ms = new MemoryStream((int)(splash.Length - splash.Position)))
                {
                    await splash.CopyToAsync(ms);
                    splashb64 = Convert.ToBase64String(ms.ToArray());
                }
            }

            return await this.Discord._rest_client.InternalModifyGuildAsync(this.Id, name, region, verification_level, default_message_notifications, afk_channel_id, afk_timeout, iconb64, owner_id, splashb64, reason);
        }

        public Task BanMemberAsync(DiscordMember member, int delete_message_days = 0, string reason = null) =>
            this.Discord._rest_client.InternalCreateGuildBanAsync(this.Id, member.Id, delete_message_days, reason);

        public Task UnbanMemberAsync(DiscordUser user, string reason = null) =>
            this.Discord._rest_client.InternalRemoveGuildBanAsync(this.Id, user.Id, reason);

        public Task LeaveAsync() =>
            this.Discord._rest_client.InternalLeaveGuildAsync(Id);

        public Task<IReadOnlyCollection<DiscordUser>> GetBansAsync() =>
            this.Discord._rest_client.InternalGetGuildBansAsync(Id);

        public Task<DiscordChannel> CreateChannelAsync(string name, ChannelType type, int? bitrate = null, int? user_limit = null, IEnumerable<DiscordOverwrite> overwrites = null, string reason = null) =>
            this.Discord._rest_client.InternalCreateGuildChannelAsync(this.Id, name, type, bitrate, user_limit, overwrites, reason);

        public Task<int> GetPruneCountAsync(int days) =>
            this.Discord._rest_client.InternalGetGuildPruneCountAsync(this.Id, days);

        public Task<int> PruneAsync(int days, string reason = null) =>
            this.Discord._rest_client.InternalBeginGuildPruneAsync(this.Id, days, reason);

        public Task<IReadOnlyCollection<DiscordIntegration>> GetIntegrationsAsync() =>
            this.Discord._rest_client.InternalGetGuildIntegrationsAsync(this.Id);

        public Task<DiscordIntegration> AttachUserIntegrationAsync(DiscordIntegration integration) =>
            this.Discord._rest_client.InternalCreateGuildIntegrationAsync(Id, integration.Type, integration.Id);

        public Task<DiscordIntegration> ModifyIntegrationAsync(DiscordIntegration integration, int expire_behaviour, int expire_grace_period, bool enable_emoticons) =>
            this.Discord._rest_client.InternalModifyGuildIntegrationAsync(Id, integration.Id, expire_behaviour, expire_grace_period, enable_emoticons);

        public Task DeleteIntegrationAsync(DiscordIntegration integration) =>
            this.Discord._rest_client.InternalDeleteGuildIntegrationAsync(Id, integration);

        public Task SyncIntegrationAsync(DiscordIntegration integration) =>
            this.Discord._rest_client.InternalSyncGuildIntegrationAsync(Id, integration.Id);

        public Task<DiscordGuildEmbed> GetEmbedAsync() =>
            this.Discord._rest_client.InternalGetGuildEmbedAsync(Id);

        public Task<IReadOnlyCollection<DiscordVoiceRegion>> GetVoiceRegionsAsync() =>
            this.Discord._rest_client.InternalGetGuildVoiceRegionsAsync(this.Id);

        public Task<IReadOnlyCollection<DiscordInvite>> GetInvitesAsync() =>
            this.Discord._rest_client.InternalGetGuildInvitesAsync(this.Id);

        public Task<DiscordInvite> CreateInviteAsync(int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false, string reason = null) =>
            this.Discord._rest_client.InternalCreateChannelInviteAsync(this.DefaultChannel.Id, max_age, max_uses, temporary, unique, reason);

        public Task<IReadOnlyCollection<DiscordWebhook>> GetWebhooksAsync() =>
            this.Discord._rest_client.InternalGetGuildWebhooksAsync(this.Id);

        public Task RemoveMemberAsync(DiscordUser user) =>
            this.Discord._rest_client.InternalRemoveGuildMemberAsync(Id, user.Id);

        public Task RemoveMemberAsync(ulong user_id) =>
            this.Discord._rest_client.InternalRemoveGuildMemberAsync(Id, user_id);

        public Task<DiscordMember> GetMemberAsync(ulong user_id) =>
            this.Discord._rest_client.InternalGetGuildMemberAsync(Id, user_id);

        public async Task<IReadOnlyCollection<DiscordMember>> GetAllMembersAsync()
        {
            var recmbr = new List<DiscordMember>(this.MemberCount + 1);

            var recd = 1000;
            var last = 0ul;
            while (recd == 1000)
            {
                var mbrs = await this.Discord._rest_client.InternalListGuildMembersAsync(this.Id, 1000, last == 0 ? null : (ulong?)last);
                recd = mbrs.Count;

                var mbr = mbrs.LastOrDefault();
                if (mbr != null)
                    last = mbr.Id;
                else
                    last = 0;

                recmbr.AddRange(mbrs);
            }

            var recids = recmbr.Select(xm => xm.Id);

            // clear the cache of users who weren't received
            for (int i = 0; i < this._members.Count; i++)
                if (!recids.Contains(this._members[i].Id))
                    this._members.RemoveAt(i--);

            var curids = this._members.Select(xm => xm.Id);

            // ignore members who already exist in the cache
            var newmbr = recmbr.Where(xm => !curids.Contains(xm.Id))
                .Select(xm => { xm.Discord = this.Discord; xm._guild_id = this.Id; return xm; });

            // add new members
            this._members.AddRange(newmbr);
            this.MemberCount = this._members.Count;

            return this.Members;
        }

        public Task<IReadOnlyCollection<DiscordChannel>> GetChannelsAsync() =>
            this.Discord._rest_client.InternalGetGuildChannelsAsync(this.Id);

        public Task<IReadOnlyCollection<DiscordMember>> ListMembersAsync(int? limit = null, ulong? after = null) =>
            this.Discord._rest_client.InternalListGuildMembersAsync(Id, limit, after);

        public Task UpdateRoleAsync(DiscordRole role, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason = null) =>
            this.Discord._rest_client.InternalModifyGuildRoleAsync(Id, role.Id, name, permissions, color, hoist, mentionable, reason);

        public Task UpdateRolePositionAsync(DiscordRole role, int position, string reason = null)
        {
            var roles = this._roles.OrderByDescending(xr => xr.Position).ToArray();
            var pmds = new RestGuildRoleReorderPayload[roles.Length];
            for (var i = 0; i < roles.Length; i++)
            {
                pmds[i] = new RestGuildRoleReorderPayload
                {
                    RoleId = roles[i].Id,
                    Position = roles[i].Position <= position ? roles[i].Position - 1 : roles[i].Position
                };
            }

            return this.Discord._rest_client.InternalModifyGuildRolePosition(this.Id, pmds, reason);
        }

        public Task<DiscordRole> CreateRoleAsync(string name = "", Permissions? permissions = null, int? color = null, bool? hoist = null, bool? mentionable = null, string reason = null) =>
            this.Discord._rest_client.InternalCreateGuildRole(this.Id, name, permissions, color, hoist, mentionable, reason);

        public Task DeleteRoleAsync(DiscordRole role) =>
            this.Discord._rest_client.InternalDeleteRoleAsync(this.Id, role.Id);

        public DiscordRole GetRole(ulong id) =>
            this.Roles.FirstOrDefault(xr => xr.Id == id);

        public Task AddRoleAsync(ulong user_id, ulong role_id, string reason = null) =>
            this.Discord._rest_client.InternalAddGuildMemberRoleAsync(Id, user_id, role_id, reason);

        public Task RemoveRoleAsync(ulong user_id, ulong role_id, string reason) =>
            this.Discord._rest_client.InternalRemoveGuildMemberRoleAsync(Id, user_id, role_id, reason);
        #endregion
    }

    /// <summary>
    /// Represents guild verification level.
    /// </summary>
    public enum VerificationLevel : int
    {
        /// <summary>
        /// No verification. Anyone can join and chat right away.
        /// </summary>
        None = 0,

        /// <summary>
        /// Low verification level. Users are required to have a verified email attached to their account in order to be able to chat.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium verification level. Users are required to have a verified email attached to their account, and account age need to be at least 5 minutes in order to be able to chat.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// (╯°□°）╯︵ ┻━┻ verification level. Users are required to have a verified email attached to their account, account age need to be at least 5 minutes, and they need to be in the server for at least 10 minutes in order to be able to chat.
        /// </summary>
        High = 3,

        /// <summary>
        /// ┻━┻ ﾐヽ(ಠ益ಠ)ノ彡┻━┻ verification level. Users are required to have a verified phone number attached to their account.
        /// </summary>
        Highest = 4
    }

    /// <summary>
    /// Represents default notification level for a guild.
    /// </summary>
    public enum DefaultMessageNotifications : int
    {
        /// <summary>
        /// All messages will trigger push notifications.
        /// </summary>
        AllMessages = 0,

        /// <summary>
        /// Only messages that mention the user (or a role he's in) will trigger push notifications.
        /// </summary>
        MentionsOnly = 1
    }

    /// <summary>
    /// Represents multi-factor authentication level required by a guild to use administrator functionality.
    /// </summary>
    public enum MfaLevel : int
    {
        /// <summary>
        /// Multi-factor authentication is not required to use administrator functionality.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Multi-factor authentication is required to use administrator functionality.
        /// </summary>
        Enabled = 1
    }
}
