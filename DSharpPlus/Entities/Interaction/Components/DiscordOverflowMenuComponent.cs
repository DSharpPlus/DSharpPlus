using System.Collections.Generic;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an overflow menu.
/// </summary>
public sealed class DiscordOverflowMenuComponent : DiscordComponent
{
    /// <summary>
    /// The groups of items this overflow menu contains.
    /// </summary>
    [JsonProperty("groups")]
    public IReadOnlyList<DiscordOverflowMenuItemGroup> ItemGroups { get; internal set; }

    internal DiscordOverflowMenuComponent()
        => this.Type = DiscordComponentType.OverflowMenu;

    /// <summary>
    /// Creates a new overflow menu.
    /// </summary>
    /// <param name="itemGroups">The item groups to send with this overflow menu.</param>
    /// <param name="id">An optional numeric ID to send with this component.</param>
    public DiscordOverflowMenuComponent(IReadOnlyList<DiscordOverflowMenuItemGroup> itemGroups, int id = 0)
        : this()
    {
        this.ItemGroups = itemGroups;
        this.Id = id;
    }
}
