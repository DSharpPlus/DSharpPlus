namespace DSharpPlus.Commands.Exceptions;

using System;

public sealed class CommandNotFoundException : CommandsException
{
    /// <summary>
    /// The name of a command that was not found.
    /// </summary>T
    public string CommandName { get; init; }

    /// <summary>
    /// Thrown when the command service fails to find a command.
    /// </summary>
    /// <param name="commandName">The name of a command that was not found.</param>
    /// <param name="message"></param>
    public CommandNotFoundException(string commandName, string? message = null) : base(message ?? $"Command {commandName} not found.")
    {
        ArgumentNullException.ThrowIfNull(commandName, nameof(commandName));
        this.CommandName = commandName;
    }
}
