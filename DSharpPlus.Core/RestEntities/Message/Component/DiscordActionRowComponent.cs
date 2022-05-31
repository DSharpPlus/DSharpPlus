using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DSharpPlus.Core.Enums;

namespace DSharpPlus.Core.RestEntities
{
    /// <summary>
    /// An Action Row is a non-interactive container component for other types of components. It has a type: 1 and a sub-array of components of other types. You can have up to 5 Action Rows per message. An Action Row cannot contain another Action Row
    /// </summary>
    public sealed record DiscordActionRowComponent : IDiscordMessageComponent
    {
        /// <inheritdoc/>
        [JsonPropertyName("type")]
        public DiscordComponentType Type { get; init; }

        /// <remarks>
        /// Cannot contain another action row.
        /// </remarks>
        [JsonPropertyName("components")]
        public IReadOnlyList<IDiscordMessageComponent> Components { get; init; } = Array.Empty<IDiscordMessageComponent>();
    }
}
