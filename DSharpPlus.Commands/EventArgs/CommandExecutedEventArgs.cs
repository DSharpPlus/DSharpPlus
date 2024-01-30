namespace DSharpPlus.Commands.EventArgs;

using DSharpPlus.AsyncEvents;
using DSharpPlus.Commands.Commands;

public sealed class CommandExecutedEventArgs : AsyncEventArgs
{
    public required CommandContext Context { get; init; }
    public required object? ReturnValue { get; init; }
    public required object? CommandObject { get; init; }
}
