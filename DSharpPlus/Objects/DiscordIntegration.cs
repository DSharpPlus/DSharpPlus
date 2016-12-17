using System;
using Newtonsoft.Json;

namespace DSharpPlus.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordIntegration : SnowflakeObject
    {
        /// <summary>
        /// Integration name
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }
        /// <summary>
        /// Integration type
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; internal set; }
        /// <summary>
        /// Is this integration enabled
        /// </summary>
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Enabled { get; internal set; }
        /// <summary>
        /// Is this integration syncing
        /// </summary>
        [JsonProperty("syncing", NullValueHandling = NullValueHandling.Ignore)]
        public bool Syncing { get; internal set; }
        /// <summary>
        /// ID that this integration uses for "subscribers"
        /// </summary>
        [JsonProperty("role_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong RoleID { get; internal set; }
        /// <summary>
        /// The behavior of expiring subscribers
        /// </summary>
        [JsonProperty("expire_behavior", NullValueHandling = NullValueHandling.Ignore)]
        public int ExpireBehavior { get; internal set; }
        /// <summary>
        /// The grace period before expiring subscribers
        /// </summary>
        [JsonProperty("expire_grace_period", NullValueHandling = NullValueHandling.Ignore)]
        public int ExpireGracePeriod { get; internal set; }
        /// <summary>
        /// User for this integration
        /// </summary>
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Integration account information
        /// </summary>
        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordIntegrationAccount Account { get; internal set; }
        /// <summary>
        /// When this integration was last synced
        /// </summary>
        [JsonProperty("synced_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime SyncedAt { get; internal set; }
    }
}
