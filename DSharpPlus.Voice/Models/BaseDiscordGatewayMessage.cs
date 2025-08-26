// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class BaseDiscordGatewayMessage
{
    public int OpCode { get; set; }
    [JsonPropertyName("t")]
    public string Type { get; set; }
}
