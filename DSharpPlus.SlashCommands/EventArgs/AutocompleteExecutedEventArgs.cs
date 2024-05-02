
using System;
using DSharpPlus.AsyncEvents;

namespace DSharpPlus.SlashCommands.EventArgs;
/// <summary>
/// Represents arguments for a <see cref="SlashCommandsExtension.AutocompleteExecuted"/> event.
/// </summary>
public class AutocompleteExecutedEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The context of the autocomplete.
    /// </summary>
    public AutocompleteContext Context { get; internal set; }

    /// <summary>
    /// The type of the provider.
    /// </summary>
    public Type ProviderType { get; internal set; }
}
