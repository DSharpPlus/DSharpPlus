// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoiceUserData
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
}
