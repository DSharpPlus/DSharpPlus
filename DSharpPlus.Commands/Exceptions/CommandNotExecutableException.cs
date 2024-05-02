using System;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Exceptions;

public sealed class CommandNotExecutableException : CommandsException
{
    public Command Command { get; init; }

    public CommandNotExecutableException(Command command, string? message = null) : base(message ?? $"Command {command.Name} is not executable.")
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        Command = command;
    }
}
