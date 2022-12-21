using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    public interface IInternalMessageComponent
    {
        /// <summary>
        /// The type of component.
        /// </summary>
        [JsonPropertyName("type")]
        InternalComponentType Type { get; init; }
    }
}
