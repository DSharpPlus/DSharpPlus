using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an object in Discord API.
    /// </summary>
    public abstract class SnowflakeObject : PropertyChangedBase
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
            => new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Id >> 22);

        /// <summary>
        /// Gets the client instance this object is tied to.
        /// </summary>
        [JsonIgnore]
        public BaseDiscordClient Discord { get; set; }

        internal SnowflakeObject() { }
    }
}
