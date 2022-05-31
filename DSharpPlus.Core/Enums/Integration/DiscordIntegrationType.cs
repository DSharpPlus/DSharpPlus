using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Core.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DiscordIntegrationType
    {
        Twitch,
        Youtube,
        Discord
    }
}
