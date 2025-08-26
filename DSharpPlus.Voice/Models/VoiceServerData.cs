// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoiceServerData
{
    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("guild_id")]
    public string GuildId { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }
}
