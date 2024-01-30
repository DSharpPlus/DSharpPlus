namespace DSharpPlus.Commands.Exceptions;

using System;
using DSharpPlus.Commands.Trees;

public sealed class CommandNotExecutableException : CommandsException
{
    /// <summary>
    /// Command.
    /// </summary>
    public Command Command { get; init; }

    /// <summary>
    /// Thrown when command is not executable
    /// </summary>
    /// <param name="command">Command.</param>
    /// <param name="message">Custom exception message.</param>
    public CommandNotExecutableException(Command command, string? message = null) : base(message ?? $"Command {command.Name} is not executable.")
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        this.Command = command;
    }
}
