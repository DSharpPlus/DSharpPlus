using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Core.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DiscordStatusType
    {
        [JsonProperty("online")]
        Online,

        [JsonProperty("dnd")]
        DoNotDisturb,

        [JsonProperty("idle")]
        AFK,

        [JsonProperty("invisible")]
        Invisible,

        [JsonProperty("offline")]
        Offline
    }
}
