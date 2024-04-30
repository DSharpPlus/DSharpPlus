namespace DSharpPlus.VoiceNext.Entities;
using Newtonsoft.Json;

internal class VoiceSelectProtocolPayloadData
{
    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("port")]
    public ushort Port { get; set; }

    [JsonProperty("mode")]
    public string Mode { get; set; }
}
