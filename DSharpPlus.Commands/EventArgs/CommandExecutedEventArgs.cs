using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.EventArgs;

public sealed class CommandExecutedEventArgs : DiscordEventArgs
{
    public required CommandContext Context { get; init; }
    public required object? CommandObject { get; init; }
}
