using Newtonsoft.Json;

namespace DSharpPlus.VideoNext.Entities
{
    internal sealed class VideoDispatch
    {
        [JsonProperty("op")]
        public int OpCode { get; set; }

        [JsonProperty("d")]
        public object Payload { get; set; }

        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
        public int? Sequence { get; set; }

        [JsonProperty("t", NullValueHandling = NullValueHandling.Ignore)]
        public string EventName { get; set; }
    }
}