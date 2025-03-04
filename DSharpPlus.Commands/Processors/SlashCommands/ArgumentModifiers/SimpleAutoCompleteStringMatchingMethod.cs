using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

/// <summary>
///   Represents the string matching method for a
///   <see cref="SimpleAutoCompleteProvider"/>.
/// </summary>
public enum SimpleAutoCompleteStringMatchingMethod
{
    /// <summary>
    ///   The <see cref="DiscordAutoCompleteChoice.Name"/> starts with the
    ///   user input.
    /// </summary>
    StartsWith,

    /// <summary>
    ///   The <see cref="DiscordAutoCompleteChoice.Name"/> contains the
    ///   user input.
    /// </summary>
    Contains,

    /// <summary>
    ///   The <see cref="DiscordAutoCompleteChoice.Name"/> fuzzy matches
    ///   the user input.
    /// </summary>
    Fuzzy
}
