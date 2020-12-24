using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.VideoNext.Entities
{
    internal class VideoSelectProtocolPayloadData
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("port")]
        public ushort Port { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }
        
    }
}