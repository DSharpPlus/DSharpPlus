using System;
using DSharpPlus.AsyncEvents;

namespace DSharpPlus.SlashCommands.EventArgs;

/// <summary>
/// Represents arguments for a <see cref="SlashCommandsExtension.AutocompleteErrored"/> event.
/// </summary>
public class AutocompleteErrorEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The exception thrown.
    /// </summary>
    public Exception Exception { get; internal set; }

    /// <summary>
    /// The context of the autocomplete.
    /// </summary>
    public AutocompleteContext Context { get; internal set; }

    /// <summary>
    /// The type of the provider.
    /// </summary>
    public Type ProviderType { get; internal set; }
}
