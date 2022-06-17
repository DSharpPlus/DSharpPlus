using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.RestEntities
{
    public interface IDiscordMessageComponent
    {
        /// <summary>
        /// The type of component.
        /// </summary>
        [JsonPropertyName("type")]
        DiscordComponentType Type { get; init; }
    }
}
