
using DSharpPlus.AsyncEvents;

namespace DSharpPlus.Commands.EventArgs;
public sealed class CommandExecutedEventArgs : AsyncEventArgs
{
    public required CommandContext Context { get; init; }
    public required object? CommandObject { get; init; }
}
