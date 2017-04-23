using System;
using System.Collections.Generic;
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
        public bool IsOwner => (OwnerID == DiscordClient._me.Id);

        #region Guild Functions
        public async Task<DiscordGuild> Delete() => await DiscordClient.InternalDeleteGuild(Id);
        public async Task<DiscordGuild> Modify(string name = "", string region = "", int verification_level = -1, int default_message_notifications = -1,
            ulong afk_channel_id = 0, int afk_timeout = -1, ulong owner_id = 0, string splash = "") 
            => await DiscordClient.InternalModifyGuild(Id, name, region, verification_level, DefaultMessageNotifications, afk_channel_id, afk_timeout, "", owner_id);
        public async Task BanMember(DiscordMember member) => await DiscordClient.InternalCreateGuildBan(Id, member.User.Id);
        public async Task UnbanMember(DiscordMember member) => await DiscordClient.InternalRemoveGuildBan(Id, member.User.Id);
        public async Task Leave() => await DiscordClient.InternalLeaveGuild(Id);
        public async Task<List<DiscordMember>> GetBans() => await DiscordClient.InternalGetGuildBans(Id);
        public async Task<DiscordChannel> CreateChannel(string name, ChannelType type, int bitrate = 0, int user_limit = 0) => await DiscordClient.InternalCreateChannel(Id, name, type, bitrate, user_limit);
        public async Task<int> GetPruneCount(int days) => await DiscordClient.InternalGetGuildPruneCount(Id, days);
        public async Task<int> Prune(int days) => await DiscordClient.InternalBeginGuildPrune(Id, days);
        public async Task<List<DiscordIntegration>> GetIntegrations() => await DiscordClient.InternalGetGuildIntegrations(Id);
        public async Task<DiscordIntegration> AttachUserIntegration(DiscordIntegration integration) => await DiscordClient.InternalCreateGuildIntegration(Id, integration.Type, integration.Id);
        public async Task<DiscordIntegration> ModifyIntegration(DiscordIntegration integration, int expire_behaviour, int expire_grace_period, bool enable_emoticons) =>
            await DiscordClient.InternalModifyGuildIntegration(Id, integration.Id, expire_behaviour, expire_grace_period, enable_emoticons);
        public async Task DeleteIntegration(DiscordIntegration integration) => await DiscordClient.InternalDeleteGuildIntegration(Id, integration);
        public async Task SyncIntegration(DiscordIntegration integration) => await DiscordClient.InternalSyncGuildIntegration(Id, integration.Id);
        public async Task<DiscordGuildEmbed> GetEmbed() => await DiscordClient.InternalGetGuildEmbed(Id);
        public async Task<List<DiscordVoiceRegion>> GetVoiceRegions() => await DiscordClient.InternalGetGuildVoiceRegions(Id);
        public async Task<List<DiscordInvite>> GetInvites() => await DiscordClient.InternalGetGuildInvites(Id);
        public async Task<List<DiscordWebhook>> GetWebhooks() => await DiscordClient.InternalGetGuildWebhooks(Id);
        public async Task RemoveMember(DiscordUser user) => await DiscordClient.InternalRemoveGuildMember(Id, user.Id);
        public async Task RemoveMember(ulong user_id) => await DiscordClient.InternalRemoveGuildMember(Id, user_id);
        public async Task<DiscordMember> GetMember(ulong user_id) => await DiscordClient.InternalGetGuildMember(Id, user_id);
        public async Task<List<DiscordMember>> GetAllMembers() => await DiscordClient.InternalGetGuildMembers(Id, MemberCount);
        public async Task ModifyMember(ulong member_id, string nickname, List<ulong> roles, bool muted, bool deaf, ulong voicechannel_id) =>
            await DiscordClient.InternalModifyGuildMember(Id, member_id, nickname, roles, muted, deaf, voicechannel_id);
        public async Task<List<DiscordChannel>> GetChannels() => await DiscordClient.InternalGetGuildChannels(Id);
        public async Task<List<DiscordMember>> ListMembers(int limit, int after) => await DiscordClient.InternalListGuildMembers(Id, limit, after);
        public async Task UpdateRole(DiscordRole role) => await DiscordClient.InternalModifyGuildRole(Id, role.Id, role.Name, role.Permissions, role.Position, role.Color, false, role.Mentionable);
        public async Task<DiscordRole> CreateRole() => await DiscordClient.InternalCreateGuildRole(Id);
        public async Task AddRole(ulong user_id, ulong role_id) => await DiscordClient.InternalAddGuildMemberRole(Id, user_id, role_id);
        public async Task RemoveRole(ulong user_id, ulong role_id) => await DiscordClient.InternalRemoveGuildMemberRole(Id, user_id, role_id);
        #endregion

    }
}
