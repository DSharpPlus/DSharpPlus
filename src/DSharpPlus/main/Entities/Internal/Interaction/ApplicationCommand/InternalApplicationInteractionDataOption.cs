using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// All options have names, and an option can either be a parameter and input value--in which case value will be set--or it can denote a subcommand or group--in which case it will contain a top-level key and another array of options.
/// </summary>
public sealed record InternalApplicationInteractionDataOption
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The value of <see cref="DiscordApplicationCommandOptionType"/>.
    /// </summary>
    [JsonPropertyName("type")]
    public DiscordApplicationCommandOptionType Type { get; init; }

    /// <summary>
    /// The value of the option resulting from user input.
    /// </summary>
    /// <remarks>
    /// A string, integer, or double. Mutually exclusive with <see cref="Options"/>.
    /// </remarks>
    [JsonPropertyName("value")]
    public Optional<object> Value { get; init; }

    /// <summary>
    /// Only present if this option is a <see cref="DiscordApplicationCommandOptionType.SubCommand"/> or <see cref="DiscordApplicationCommandOptionType.SubCommandGroup"/>.
    /// </summary>
    /// <remarks>
    /// Mutually exclusive with <see cref="Value"/>.
    /// </remarks>
    [JsonPropertyName("options")]
    public Optional<IReadOnlyList<InternalApplicationInteractionDataOption>> Options { get; init; }

    /// <summary>
    /// True if this option is the currently focused option for autocomplete.
    /// </summary>
    [JsonPropertyName("focused")]
    public Optional<bool> Focused { get; init; }
}
