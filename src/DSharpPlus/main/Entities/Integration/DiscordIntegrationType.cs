using System.Text.Json.Serialization;

namespace DSharpPlus.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DiscordIntegrationType
{
    Twitch,
    Youtube,
    Discord
}
