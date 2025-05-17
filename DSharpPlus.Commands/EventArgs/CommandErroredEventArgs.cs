using System;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.EventArgs;

public sealed class CommandErroredEventArgs : DiscordEventArgs
{
    public required CommandContext Context { get; init; }
    public required Exception Exception { get; init; }
    public required object? CommandObject { get; init; }
}
