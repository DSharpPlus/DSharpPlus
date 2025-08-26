// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoiceIdentifyData
{
    [JsonPropertyName("server_id")]
    public string ServerId { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("max_dave_protocol_version")]
    public int MaxSupportedDaveVersion { get; set; }
}
