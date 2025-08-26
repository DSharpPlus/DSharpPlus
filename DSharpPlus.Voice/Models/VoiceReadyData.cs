// See https://aka.ms/new-console-template for more information
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class VoiceReadyData
{
    [JsonPropertyName("ssrc")]
    public uint Ssrc { get; set; }

    [JsonPropertyName("ip")]
    public string Ip { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("modes")]
    public List<string> Modes { get; set; }

    [JsonPropertyName("heartbeat_interval")]
    public int HeartbeatInterval { get; set; }
}
