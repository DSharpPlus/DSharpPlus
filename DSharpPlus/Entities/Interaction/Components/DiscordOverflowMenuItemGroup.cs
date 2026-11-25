using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a group of overflow menu items within a menu. Groups are separated by visual dashes and can have a label.
/// </summary>
[CollectionBuilder(typeof(DiscordOverflowMenuItemGroup), nameof(Create))]
public sealed class DiscordOverflowMenuItemGroup
{
    /// <summary>
    /// An optional label for this item group.
    /// </summary>
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string? Label { get; internal set; }

    /// <summary>
    /// The menu items belonging to this group.
    /// </summary>
    [JsonProperty("components")]
    public IReadOnlyList<DiscordOverflowMenuItemComponent> Items { get; internal set; }

    // this does nothing and is to prevent users from constructing this type outside of the intended construction
    internal DiscordOverflowMenuItemGroup()
    {
        
    }

    internal static DiscordOverflowMenuItemGroup Create(string? label, ReadOnlySpan<DiscordOverflowMenuItemComponent> values)
    {
        return new()
        {
            Label = label,
            Items = [.. values]
        };
    }

    internal static DiscordOverflowMenuItemGroup Create(ReadOnlySpan<DiscordOverflowMenuItemComponent> values)
        => Create(null, values);
}
