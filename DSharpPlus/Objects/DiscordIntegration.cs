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
<<<<<<< HEAD
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("name")]
>>>>>>> master
        public string Name { get; internal set; }
        /// <summary>
        /// Integration type
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("type")]
>>>>>>> master
        public string Type { get; internal set; }
        /// <summary>
        /// Is this integration enabled
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("enabled")]
>>>>>>> master
        public bool Enabled { get; internal set; }
        /// <summary>
        /// Is this integration syncing
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("syncing", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("syncing")]
>>>>>>> master
        public bool Syncing { get; internal set; }
        /// <summary>
        /// ID that this integration uses for "subscribers"
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("role_id", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("role_id")]
>>>>>>> master
        public ulong RoleID { get; internal set; }
        /// <summary>
        /// The behavior of expiring subscribers
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("expire_behavior", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("expire_behavior")]
>>>>>>> master
        public int ExpireBehavior { get; internal set; }
        /// <summary>
        /// The grace period before expiring subscribers
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("expire_grace_period", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("expire_grace_period")]
>>>>>>> master
        public int ExpireGracePeriod { get; internal set; }
        /// <summary>
        /// User for this integration
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("user")]
>>>>>>> master
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Integration account information
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("account")]
>>>>>>> master
        public DiscordIntegrationAccount Account { get; internal set; }
        /// <summary>
        /// When this integration was last synced
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("synced_at", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("synced_at")]
>>>>>>> master
        public DateTime SyncedAt { get; internal set; }
    }
}
