// See https://aka.ms/new-console-template for more information
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class VoiceUserIdsData
{
    [JsonPropertyName("user_ids")]
    public List<string> UserIds { get; set; }
}
