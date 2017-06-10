using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        public DiscordMember Owner => this.Members.FirstOrDefault(xm => xm.Id == this.OwnerId) ?? this.Discord._rest_client.InternalGetGuildMember(this.Id, this.OwnerId).GetAwaiter().GetResult();

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
        /// Gets this guild's creation date.
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime JoinedAt { get; internal set; }

        /// <summary>
        /// Gets whether this guild is considered tp be a large guild.
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
        public IReadOnlyList<DiscordVoiceState> VoiceStates { get; internal set; }
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
        public Task<DiscordGuild> DeleteAsync() =>
            this.Discord._rest_client.InternalDeleteGuild(Id);

        public Task<DiscordGuild> ModifyAsync(string name = "", string region = "", int verification_level = -1, int default_message_notifications = -1, ulong afk_channel_id = 0, int afk_timeout = -1, ulong owner_id = 0, string splash = "") =>
            this.Discord._rest_client.InternalModifyGuild(Id, name, region, verification_level, DefaultMessageNotifications, afk_channel_id, afk_timeout, "", owner_id);

        public Task BanMemberAsync(DiscordMember member) =>
            this.Discord._rest_client.InternalCreateGuildBan(Id, member.Id);

        public Task UnbanMemberAsync(DiscordMember member) =>
            this.Discord._rest_client.InternalRemoveGuildBan(Id, member.Id);

        public Task LeaveAsync() =>
            this.Discord._rest_client.InternalLeaveGuild(Id);

        public Task<List<DiscordMember>> GetBansAsync() =>
            this.Discord._rest_client.InternalGetGuildBans(Id);

        public Task<DiscordChannel> CreateChannelAsync(string name, ChannelType type, int bitrate = 0, int user_limit = 0) =>
            this.Discord._rest_client.InternalCreateChannel(Id, name, type, bitrate, user_limit);

        public Task<int> GetPruneCountAsync(int days) =>
            this.Discord._rest_client.InternalGetGuildPruneCount(Id, days);

        public Task<int> PruneAsync(int days) =>
            this.Discord._rest_client.InternalBeginGuildPrune(Id, days);

        public Task<List<DiscordIntegration>> GetIntegrationsAsync() =>
            this.Discord._rest_client.InternalGetGuildIntegrations(Id);

        public Task<DiscordIntegration> AttachUserIntegrationAsync(DiscordIntegration integration) =>
            this.Discord._rest_client.InternalCreateGuildIntegration(Id, integration.Type, integration.Id);

        public Task<DiscordIntegration> ModifyIntegrationAsync(DiscordIntegration integration, int expire_behaviour, int expire_grace_period, bool enable_emoticons) =>
            this.Discord._rest_client.InternalModifyGuildIntegration(Id, integration.Id, expire_behaviour, expire_grace_period, enable_emoticons);

        public Task DeleteIntegrationAsync(DiscordIntegration integration) =>
            this.Discord._rest_client.InternalDeleteGuildIntegration(Id, integration);

        public Task SyncIntegrationAsync(DiscordIntegration integration) =>
            this.Discord._rest_client.InternalSyncGuildIntegration(Id, integration.Id);

        public Task<DiscordGuildEmbed> GetEmbedAsync() =>
            this.Discord._rest_client.InternalGetGuildEmbed(Id);

        public Task<List<DiscordVoiceRegion>> GetVoiceRegionsAsync() =>
            this.Discord._rest_client.InternalGetGuildVoiceRegions(Id);

        public Task<List<DiscordInvite>> GetInvitesAsync() =>
            this.Discord._rest_client.InternalGetGuildInvites(Id);

        public Task<List<DiscordWebhook>> GetWebhooksAsync() =>
            this.Discord._rest_client.InternalGetGuildWebhooks(Id);

        public Task RemoveMemberAsync(DiscordUser user) =>
            this.Discord._rest_client.InternalRemoveGuildMember(Id, user.Id);

        public Task RemoveMemberAsync(ulong user_id) =>
            this.Discord._rest_client.InternalRemoveGuildMember(Id, user_id);

        public Task<DiscordMember> GetMemberAsync(ulong user_id) =>
            this.Discord._rest_client.InternalGetGuildMember(Id, user_id);

        public Task<List<DiscordMember>> GetAllMembersAsync() =>
            this.Discord._rest_client.InternalGetGuildMembers(Id, MemberCount);

        public Task ModifyMemberAsync(ulong member_id, string nickname, List<ulong> roles, bool muted, bool deaf, ulong voicechannel_id) =>
            this.Discord._rest_client.InternalModifyGuildMember(Id, member_id, nickname, roles, muted, deaf, voicechannel_id);

        public Task<List<DiscordChannel>> GetChannelsAsync() =>
            this.Discord._rest_client.InternalGetGuildChannels(Id);

        public Task<List<DiscordMember>> ListMembersAsync(int limit, int after) =>
            this.Discord._rest_client.InternalListGuildMembers(Id, limit, after);

        public Task UpdateRoleAsync(DiscordRole role) =>
            this.Discord._rest_client.InternalModifyGuildRole(Id, role.Id, role.Name, role.Permissions, role.Position, role.Color, false, role.Mentionable);

        public Task UpdateRolePositionAsync(DiscordRole role, int position) =>
            this.Discord._rest_client.InternalModifyGuildRolePosition(this.Id, role.Id, position);

        public Task<DiscordRole> CreateRoleAsync() =>
            this.Discord._rest_client.InternalCreateGuildRole(Id);

        public Task DeleteRoleAsync(DiscordRole role) =>
            this.Discord._rest_client.InternalDeleteRole(this.Id, role.Id);

        public DiscordRole GetRole(ulong id) =>
            this.Roles.FirstOrDefault(xr => xr.Id == id);

        public Task AddRoleAsync(ulong user_id, ulong role_id) =>
            this.Discord._rest_client.InternalAddGuildMemberRole(Id, user_id, role_id);

        public Task RemoveRoleAsync(ulong user_id, ulong role_id) =>
            this.Discord._rest_client.InternalRemoveGuildMemberRole(Id, user_id, role_id);
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
