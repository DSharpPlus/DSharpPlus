using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordGuild : SnowflakeObject
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("icon")]
        public string Icon { get; internal set; }
        [JsonProperty("")]
        public string IconUrl => $"";
        [JsonProperty("splash")]
        public string Splash { get; internal set; }
        [JsonProperty("owner_id")]
        public ulong OwnerID { get; internal set; }
        [JsonProperty("voice_region")]
        public DiscordVoiceRegion VoiceRegion { get; internal set; }
        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong AFKChannelID { get; internal set; } = 0;
        [JsonProperty("afk_timeout")]
        public int AFKTimeout { get; internal set; }
        [JsonProperty("embed_enabled")]
        public bool EmbedEnabled { get; internal set; }
        [JsonProperty("embed_channel_id")]
        public ulong EmbedChannelID { get; internal set; }
        [JsonProperty("verification_level")]
        public int VerificationLevel { get; internal set; }
        [JsonProperty("default_message_notifications")]
        public int DefaultMessageNotifications { get; internal set; }
        [JsonProperty("roles")]
        public List<DiscordRole> Roles { get; internal set; }
        [JsonProperty("emojis")]
        public List<DiscordEmoji> Emojis { get; internal set; }
        [JsonProperty("features")]
        public List<string> Features { get; internal set; }
        [JsonProperty("mfa_level")]
        public int MFALevel { get; internal set; }
        [JsonProperty("joined_at")]
        public DateTime JoinedAt { get; internal set; }
        [JsonProperty("is_large")]
        public bool IsLarge { get; internal set; }
        [JsonProperty("unavailable")]
        public bool Unavailable { get; internal set; }
        [JsonProperty("member_count")]
        public int MemberCount { get; internal set; }
        [JsonProperty("voice_states")]
        public List<DiscordVoiceState> VoiceStates { get; internal set; }
        [JsonProperty("members")]
        public List<DiscordMember> Members { get; internal set; }
        [JsonProperty("channels")]
        public List<DiscordChannel> Channels => DiscordClient.InternalGetGuildChannels(ID).Result;
        [JsonProperty("presences")]
        public List<object> Presences { get; internal set; }
        [JsonProperty("is_owner")]
        public bool IsOwner { get; internal set; }
        [JsonProperty("permissions")]
        public ulong Permissions { get; internal set; }

        internal static DiscordGuild FromJson(JObject GuildObject)
        {
            return new DiscordGuild();
        }

        internal static DiscordGuild FromJson(string GuildObject) => DiscordGuild.FromJson(JObject.Parse(GuildObject));

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
        #endregion

    }
}
