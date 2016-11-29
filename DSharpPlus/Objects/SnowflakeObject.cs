using Newtonsoft.Json;
using System;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class SnowflakeObject
    {
        /// <summary>
        /// The ID of the current object
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ID { get; internal set; }
        /// <summary>
        /// The create date of the current object
        /// </summary>
        [JsonIgnore]
        public DateTime CreationDate => new DateTime(2015, 1, 1).AddMilliseconds(this.ID >> 22);

        internal SnowflakeObject() { }
    }
}
