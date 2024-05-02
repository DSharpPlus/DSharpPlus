using Newtonsoft.Json;

namespace DSharpPlus.VoiceNext.Entities;

internal sealed class VoiceSessionDescriptionPayload
{
    [JsonProperty("secret_key")]
    public byte[] SecretKey { get; set; }

    [JsonProperty("mode")]
    public string Mode { get; set; }
}
