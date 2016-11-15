using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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
        public List<DiscordChannel> Channels { get; internal set; } = new List<DiscordChannel>();
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
    }
}
