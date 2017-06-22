using System;
using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SnowflakeObject
    {
        /// <summary>
        /// The ID of the current object
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; internal set; }
        /// <summary>
        /// The create date of the current object
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset CreationDate => new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Id >> 22);

        /// <summary>
        /// The client instance this object is tied to.
        /// </summary>
        [JsonIgnore]
        internal DiscordClient Discord { get; set; }

        internal SnowflakeObject() { }
    }
}
