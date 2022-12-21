using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

/// <summary>
/// The are the only valid values for a user to pick, used on <see cref="InternalApplicationCommandOption.Choices"/>.
/// </summary>
public sealed record InternalApplicationCommandOptionChoice
{
    /// <summary>
    /// A 1-32 character name that matches against the following Regex: <c>^[-_\p{L}\p{N}\p{sc=Deva}\p{sc=Thai}]{1,32}$</c> with the unicode flag set. If there is a lowercase variant of any letters used, you must use those. Characters with no lowercase variants and/or uncased letters are still allowed. <see cref="InternalApplicationCommandType.User"/> and <see cref="InternalApplicationCommandType.Message"/> commands may be mixed case and can include spaces.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Localization dictionary for the <c>name</c> field. Values follow the same restrictions as <c>name</c>.
    /// </summary>
    [JsonPropertyName("name_localizations")]
    public Optional<IReadOnlyDictionary<string, string>> NameLocalizations { get; init; }

    /// <summary>
    /// Value of the choice, up to 100 characters if string.
    /// </summary>
    /// <remarks>
    /// A string, integer, or double.
    /// </remarks>
    [JsonPropertyName("value")]
    public object Value { get; init; } = null!;
}
