using DSharpPlus.AsyncEvents;
using DSharpPlus.CommandAll.Commands;

namespace DSharpPlus.CommandAll.EventArgs
{
    public sealed class CommandExecutedEventArgs : AsyncEventArgs
    {
        public required CommandContext Context { get; init; }
        public required object? Value { get; init; }
    }
}
