using System;
using Emzi0767.Utilities;

namespace DSharpPlus.SlashCommands.EventArgs;

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
