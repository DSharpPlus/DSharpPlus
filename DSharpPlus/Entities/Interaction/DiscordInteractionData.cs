using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the inner data payload of a <see cref="DiscordInteraction"/>.
/// </summary>
public sealed class DiscordInteractionData : SnowflakeObject
{
    /// <summary>
    /// Gets the name of the invoked interaction.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the parameters and values of the invoked interaction.
    /// </summary>
    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<DiscordInteractionDataOption> Options { get; internal set; }

    /// <summary>
    /// Gets the Discord snowflake objects resolved from this interaction's arguments.
    /// </summary>
    [JsonProperty("resolved", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordInteractionResolvedCollection Resolved { get; internal set; }

    /// <summary>
    /// The Id of the component that invoked this interaction, or the Id of the modal the interaction was spawned from.
    /// </summary>
    [JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
    public string CustomId { get; internal set; }

    /// <summary>
    /// The title of the modal, if applicable.
    /// </summary>
    [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; internal set; }

    /// <summary>
    /// Components on this interaction. Only applies to modal interactions.
    /// </summary>
    public IReadOnlyList<DiscordActionRowComponent> Components => this.components;

    [JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
    internal List<DiscordActionRowComponent> components;

    /// <summary>
    /// The Id of the target. Applicable for context menus.
    /// </summary>
    [JsonProperty("target_id", NullValueHandling = NullValueHandling.Ignore)]
    internal ulong? Target { get; set; }

    /// <summary>
    /// The type of component that invoked this interaction, if applicable.
    /// </summary>
    [JsonProperty("component_type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordComponentType ComponentType { get; internal set; }

    [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
    public string[] Values { get; internal set; } = [];

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordApplicationCommandType Type { get; internal set; }
}
