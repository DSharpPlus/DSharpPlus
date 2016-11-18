using Newtonsoft.Json;
using System;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordIntegration : SnowflakeObject
    {
        /// <summary>
        /// Integration name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
        /// <summary>
        /// Integration type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; internal set; }
        /// <summary>
        /// Is this integration enabled
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; internal set; }
        /// <summary>
        /// Is this integration syncing
        /// </summary>
        [JsonProperty("syncing")]
        public bool Syncing { get; internal set; }
        /// <summary>
        /// ID that this integration uses for "subscribers"
        /// </summary>
        [JsonProperty("role_id")]
        public ulong RoleID { get; internal set; }
        /// <summary>
        /// The behavior of expiring subscribers
        /// </summary>
        [JsonProperty("expire_behavior")]
        public int ExpireBehavior { get; internal set; }
        /// <summary>
        /// The grace period before expiring subscribers
        /// </summary>
        [JsonProperty("expire_grace_period")]
        public int ExpireGracePeriod { get; internal set; }
        /// <summary>
        /// User for this integration
        /// </summary>
        [JsonProperty("user")]
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Integration account information
        /// </summary>
        [JsonProperty("account")]
        public DiscordIntegrationAccount Account { get; internal set; }
        /// <summary>
        /// When this integration was last synced
        /// </summary>
        [JsonProperty("synced_at")]
        public DateTime SyncedAt { get; internal set; }
    }
}
