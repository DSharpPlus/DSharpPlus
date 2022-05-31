using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public interface IDiscordMessageComponent
    {
        /// <summary>
        /// The type of component.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        DiscordComponentType Type { get; init; }
    }
}
