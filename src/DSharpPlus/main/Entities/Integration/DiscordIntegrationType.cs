using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DiscordIntegrationType
    {
        Twitch,
        Youtube,
        Discord
    }
}
