namespace DSharpPlus.SlashCommands.EventArgs;

using System;
using DSharpPlus.AsyncEvents;

/// <summary>
/// Represents arguments for a <see cref="SlashCommandsExtension.SlashCommandErrored"/> event.
/// </summary>
public sealed class SlashCommandErrorEventArgs : AsyncEventArgs
{
    /// <summary>
    /// The context of the command.
    /// </summary>
    public InteractionContext Context { get; internal set; }

    /// <summary>
    /// The exception thrown.
    /// </summary>
    public Exception Exception { get; internal set; }
}
