using Newtonsoft.Json;
using System;

namespace DSharpPlus
{
    public class DiscordIntegration : SnowflakeObject
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("type")]
        public string Type { get; internal set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; internal set; }
        [JsonProperty("syncing")]
        public bool Syncing { get; internal set; }
        [JsonProperty("role_id")]
        public ulong RoleID { get; internal set; }
        [JsonProperty("expire_behavior")]
        public int ExpireBehavior { get; internal set; }
        [JsonProperty("expire_grace_period")]
        public int ExpireGracePeriod { get; internal set; }
        [JsonProperty("user")]
        public DiscordUser User { get; internal set; }
        [JsonProperty("account")]
        public DiscordIntegrationAccount Account { get; internal set; }
        [JsonProperty("synced_at")]
        public DateTime SyncedAt { get; internal set; }
    }
}
