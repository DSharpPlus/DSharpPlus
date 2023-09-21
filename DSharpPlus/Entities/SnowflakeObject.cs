using System;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an object in Discord API.
    /// </summary>
    public abstract class SnowflakeObject
    {
        /// <summary>
        /// Gets the ID of this object.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; internal set; }

        /// <summary>
        /// Gets the date and time this object was created.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset CreationTimestamp
            => this.Id.GetSnowflakeTime();

        /// <summary>
        /// Gets the client instance this object is tied to.
        /// </summary>
        [JsonIgnore]
        internal BaseDiscordClient Discord { get; set; }

        internal SnowflakeObject() { }
    }
}
