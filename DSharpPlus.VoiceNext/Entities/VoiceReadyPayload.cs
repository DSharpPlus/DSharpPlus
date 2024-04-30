namespace DSharpPlus.VoiceNext.Entities;
using System.Collections.Generic;
using Newtonsoft.Json;

internal sealed class VoiceReadyPayload
{
    [JsonProperty("ssrc")]
    public uint SSRC { get; set; }

    [JsonProperty("ip")]
    public string Address { get; set; }

    [JsonProperty("port")]
    public ushort Port { get; set; }

    [JsonProperty("modes")]
    public IReadOnlyList<string> Modes { get; set; }

    [JsonProperty("heartbeat_interval")]
    public int HeartbeatInterval { get; set; }
}
