using Newtonsoft.Json;
using System;

namespace DSharpPlus
{
    public class SnowflakeObject
    {
        [JsonProperty("id")]
        public ulong ID { get; internal set; }
        [JsonIgnore]
        public DateTime CreationDate => new DateTime(2015, 1, 1).AddMilliseconds(this.ID >> 22);
    }
}
