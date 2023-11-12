namespace DSharpPlus.CommandAll.EventArgs;

using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Commands;

public sealed class CommandExecutedEventArgs : AsyncEventArgs
{
    public required CommandContext Context { get; init; }
    public required object? ReturnValue { get; init; }
    public required object? CommandObject { get; init; }
}
