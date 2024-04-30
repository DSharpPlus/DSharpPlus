namespace DSharpPlus.VoiceNext.Entities;

using Newtonsoft.Json;

internal sealed class VoiceSelectProtocolPayload
{
    [JsonProperty("protocol")]
    public string Protocol { get; set; }

    [JsonProperty("data")]
    public VoiceSelectProtocolPayloadData Data { get; set; }
}
