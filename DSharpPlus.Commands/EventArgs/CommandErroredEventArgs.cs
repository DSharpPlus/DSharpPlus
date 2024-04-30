namespace DSharpPlus.Commands.EventArgs;

using System;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands;

public sealed class CommandErroredEventArgs : AsyncEventArgs
{
    public required CommandContext Context { get; init; }
    public required Exception Exception { get; init; }
    public required object? CommandObject { get; init; }
}
