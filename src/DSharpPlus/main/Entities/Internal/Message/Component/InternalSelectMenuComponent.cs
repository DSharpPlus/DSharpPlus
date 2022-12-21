using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalSelectMenuComponent : IInternalMessageComponent
{
    /// <inheritdoc/>
    [JsonPropertyName("type")]
    public DiscordComponentType Type { get; init; }

    /// <summary>
    /// A developer-defined identifier for the select menu, max 100 characters.
    /// </summary>
    [JsonPropertyName("custom_id")]
    public string CustomId { get; init; } = null!;

    /// <summary>
    /// The choices in the select, max 25.
    /// </summary>
    [JsonPropertyName("options")]
    public IReadOnlyList<InternalSelectMenuOptionComponent> Options { get; init; } = Array.Empty<InternalSelectMenuOptionComponent>();

    /// <summary>
    /// The custom placeholder text if nothing is selected, max 150 characters.
    /// </summary>
    [JsonPropertyName("placeholder")]
    public Optional<string> Placeholder { get; init; }

    /// <summary>
    /// The minimum number of items that must be chosen; default 1, min 0, max 25.
    /// </summary>
    [JsonPropertyName("min_values")]
    public Optional<int> MinValues { get; init; }

    /// <summary>
    /// The maximum number of items that can be chosen; default 1, max 25.
    /// </summary>
    [JsonPropertyName("max_values")]
    public Optional<int> MaxValues { get; init; }

    /// <summary>
    /// Whether to disable the select, default false.
    /// </summary>
    [JsonPropertyName("disabled")]
    public Optional<bool> Disabled { get; init; }
}
