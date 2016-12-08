using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public string IconUrl => $"https://cdn.discordapp.com/icons/{ID}/{Icon}.jpg";
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
        public List<DiscordChannel> Channels => DiscordClient.InternalGetGuildChannels(ID).Result;
        /// <summary>
        /// List of simple presence objects
        /// </summary>
        [JsonProperty("presences", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Presences { get; internal set; }
        /// <summary>
        /// Is the current user the guild owner
        /// </summary>
        [JsonProperty("is_owner", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOwner => (OwnerID == DiscordClient._me.ID);

        #region Guild Functions
        public async Task<DiscordGuild> Delete() => await DiscordClient.InternalDeleteGuild(ID);
        public async Task<DiscordGuild> Modify(string name = "", string region = "", int verification_level = -1, int default_message_notifications = -1,
            ulong afkchannelid = 0, int afktimeout = -1, ulong ownerID = 0, string splash = "") 
            => await DiscordClient.InternalModifyGuild(ID, name, region, verification_level, DefaultMessageNotifications, afkchannelid, afktimeout, "", ownerID, "");
        public async Task BanMember(DiscordMember Member) => await DiscordClient.InternalCreateGuildBan(ID, Member.User.ID);
        public async Task UnbanMember(DiscordMember Member) => await DiscordClient.InternalRemoveGuildBan(ID, Member.User.ID);
        public async Task Leave() => await DiscordClient.InternalLeaveGuild(ID);
        public async Task<List<DiscordMember>> GetBans() => await DiscordClient.InternalGetGuildBans(ID);
        public async Task<DiscordChannel> CreateChannel(string Name, ChannelType type, int bitrate = 0, int userlimit = 0) => await DiscordClient.InternalCreateChannel(ID, Name, type, bitrate, userlimit);
        public async Task<int> GetPruneCount(int days) => await DiscordClient.InternalGetGuildPruneCount(ID, days);
        public async Task<int> Prune(int days) => await DiscordClient.InternalBeginGuildPrune(ID, days);
        public async Task<List<DiscordIntegration>> GetIntegrations() => await DiscordClient.InternalGetGuildIntegrations(ID);
        public async Task<DiscordIntegration> AttachUserIntegration(DiscordIntegration integration) => await DiscordClient.InternalCreateGuildIntegration(ID, integration.Type, integration.ID);
        public async Task<DiscordIntegration> ModifyIntegration(DiscordIntegration integration, int expire_behaviour, int expire_grace_period, bool enable_emoticons) =>
            await DiscordClient.InternalModifyGuildIntegration(ID, integration.ID, expire_behaviour, expire_grace_period, enable_emoticons);
        public async Task DeleteIntegration(DiscordIntegration integration) => await DiscordClient.InternalDeleteGuildIntegration(ID, integration);
        public async Task SyncIntegration(DiscordIntegration integration) => await DiscordClient.InternalSyncGuildIntegration(ID, integration.ID);
        public async Task<DiscordGuildEmbed> GetEmbed() => await DiscordClient.InternalGetGuildEmbed(ID);
        public async Task<List<DiscordVoiceRegion>> GetVoiceRegions() => await DiscordClient.InternalGetGuildVoiceRegions(ID);
        public async Task<List<DiscordInvite>> GetInvites() => await DiscordClient.InternalGetGuildInvites(ID);
        public async Task<List<DiscordWebhook>> GetWebhooks() => await DiscordClient.InternalGetGuildWebhooks(ID);
        public async Task RemoveMember(DiscordUser user) => await DiscordClient.InternalRemoveGuildMember(ID, user.ID);
        public async Task RemoveMember(ulong UserID) => await DiscordClient.InternalRemoveGuildMember(ID, UserID);
        #endregion

    }
}
