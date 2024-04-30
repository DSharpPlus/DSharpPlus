namespace DSharpPlus.Commands.EventArgs;

using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands;

public sealed class CommandExecutedEventArgs : AsyncEventArgs
{
    public required CommandContext Context { get; init; }
    public required object? CommandObject { get; init; }
}
