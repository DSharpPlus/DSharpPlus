namespace DSharpPlus.Commands.EventArgs;

using System;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Trees;

/// <summary>
/// Represents arguments for <see cref="CommandsExtension.CommandErrored"/> event.
/// </summary>
public sealed class CommandErroredEventArgs : AsyncEventArgs
{
    public required CommandContext Context { get; init; }
    public required Exception Exception { get; init; }
    public required object? CommandObject { get; init; }
}
