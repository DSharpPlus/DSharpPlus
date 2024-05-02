using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.Entities;

internal sealed class VoiceSelectProtocolPayload
{
    [JsonProperty("protocol")]
    public string Protocol { get; set; }

    [JsonProperty("data")]
    public VoiceSelectProtocolPayloadData Data { get; set; }
}
