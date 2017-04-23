using Newtonsoft.Json;

namespace DSharpPlus.Objects.Transport
{
    internal class TransportUser
    {
        [JsonProperty("id")]
        public ulong Id { get; internal set; }
    }
}
