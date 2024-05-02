using System;
using DSharpPlus.AsyncEvents;

namespace DSharpPlus.Commands.EventArgs;

public sealed class CommandErroredEventArgs : AsyncEventArgs
{
    public required CommandContext Context { get; init; }
    public required Exception Exception { get; init; }
    public required object? CommandObject { get; init; }
}
