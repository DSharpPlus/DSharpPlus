using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public interface IInternalMessageComponent
{
    /// <summary>
    /// The type of component.
    /// </summary>
    [JsonPropertyName("type")]
    DiscordComponentType Type { get; init; }
}
