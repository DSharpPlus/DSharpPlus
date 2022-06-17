using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DiscordStatusType
    {
        [EnumMember(Value = "online")]
        Online,

        [EnumMember(Value = "dnd")]
        DoNotDisturb,

        [EnumMember(Value = "idle")]
        AFK,

        [EnumMember(Value = "invisible")]
        Invisible,

        [EnumMember(Value = "offline")]
        Offline
    }
}
