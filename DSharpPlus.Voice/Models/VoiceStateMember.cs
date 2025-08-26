// See https://aka.ms/new-console-template for more information
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class VoiceStateMember
{
    [JsonPropertyName("user")]
    public VoiceStateUser User { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; }

    [JsonPropertyName("premium_since")]
    public string PremiumSince { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("nick")]
    public string Nick { get; set; }

    [JsonPropertyName("mute")]
    public bool Mute { get; set; }

    [JsonPropertyName("joined_at")]
    public string JoinedAt { get; set; }

    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("deaf")]
    public bool Deaf { get; set; }

    [JsonPropertyName("communication_disabled_until")]
    public string CommunicationDisabledUntil { get; set; }

    [JsonPropertyName("banner")]
    public string Banner { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}
