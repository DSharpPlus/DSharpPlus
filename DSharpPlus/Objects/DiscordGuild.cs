using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordGuild : SnowflakeObject
    {
        /// <summary>
        /// Guild name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }
        /// <summary>
        /// Icon hash
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string Icon { get; internal set; }
        /// <summary>
        /// Icon Url
        /// </summary>
        [JsonIgnore]
        public string IconUrl => $"https://cdn.discordapp.com/icons/{Id}/{Icon}.jpg";
        /// <summary>
        /// Splash hash
        /// </summary>
        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string Splash { get; internal set; }
        /// <summary>
        /// ID of the owner
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerID { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string RegionID { get; internal set; }
        /// <summary>
        /// ID of the afk channel
        /// </summary>
        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong AFKChannelID { get; internal set; } = 0;
        /// <summary>
        /// AFK timeout in seconds
        /// </summary>
        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int AFKTimeout { get; internal set; }
        /// <summary>
        /// Is this guild embeddable
        /// </summary>
        [JsonProperty("embed_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool EmbedEnabled { get; internal set; }
        /// <summary>
        /// ID of embedded channel
        /// </summary>
        [JsonProperty("embed_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong EmbedChannelID { get; internal set; }
        /// <summary>
        /// Level of verification
        /// </summary>
        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public int VerificationLevel { get; internal set; }
        /// <summary>
        /// Default message notifications level
        /// </summary>
        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public int DefaultMessageNotifications { get; internal set; }
        /// <summary>
        /// List of role objects
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordRole> Roles { get; internal set; }
        /// <summary>
        /// List of emoji objects
        /// </summary>
        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordEmoji> Emojis { get; internal set; }
        /// <summary>
        /// List of guild features
        /// </summary>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Features { get; internal set; }
        /// <summary>
        /// Required MFA level for this guild
        /// </summary>
        [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
        public int MFALevel { get; internal set; }
        /// <summary>
        /// Date this guild was joined at
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime JoinedAt { get; internal set; }
        /// <summary>
        /// Whether this is considered a large guild
        /// </summary>
        [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
        public bool Large { get; internal set; }
        /// <summary>
        /// Is this guild unavailable
        /// </summary>
        [JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
        public bool Unavailable { get; internal set; }
        /// <summary>
        /// Total number of members in this guild
        /// </summary>
        [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int MemberCount { get; internal set; }
        /// <summary>
        /// List of voice state objects
        /// </summary>
        [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordVoiceState> VoiceStates { get; internal set; }
        /// <summary>
        /// List of guild member objects
        /// </summary>
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordMember> Members { get; internal set; }
        /// <summary>
        /// List of channel objects
        /// </summary>
        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordChannel> Channels;
        /// <summary>
        /// List of simple presence objects
        /// </summary>
        [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
        public List<DiscordPresence> Presences { get; internal set; }
        /// <summary>
        /// Is the current user the guild owner
        /// </summary>
        [JsonProperty("is_owner", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOwner => (OwnerID == this.Discord.Me.Id);

        #region Guild Functions
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
}
