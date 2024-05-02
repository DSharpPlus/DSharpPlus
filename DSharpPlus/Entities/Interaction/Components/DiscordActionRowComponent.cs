using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a row of components. Action rows can have up to five components.
/// </summary>
public sealed class DiscordActionRowComponent : DiscordComponent
{
    /// <summary>
    /// The components contained within the action row.
    /// </summary>
    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<DiscordComponent> Components { get; internal set; } = [];

    public DiscordActionRowComponent(IEnumerable<DiscordComponent> components) : this() => Components = components.ToList().AsReadOnly();
    internal DiscordActionRowComponent() => Type = DiscordComponentType.ActionRow; // For Json.NET
}
