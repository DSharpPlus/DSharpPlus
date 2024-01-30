namespace DSharpPlus.Commands.EventArgs;

using System;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Trees;

/// <summary>
/// Represents arguments for <see cref="CommandsExtension.CommandErrored"/> event.
/// </summary>
public sealed class CommandErroredEventArgs : AsyncEventArgs
{
    /// <summary>
    /// Gets the context in which the command errored.
    /// </summary>
    public required CommandContext Context { get; init; }
    /// <summary>
    /// Gets the exception that got thrown.
    /// </summary>
    public required Exception Exception { get; init; }
    /// <summary>
    /// Gets the command object that errored.
    /// </summary>
    public required object? CommandObject { get; init; }
}
