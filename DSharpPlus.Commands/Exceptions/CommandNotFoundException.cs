namespace DSharpPlus.Commands.Exceptions;

using System;

public sealed class CommandNotFoundException : CommandsException
{
    public string CommandName { get; init; }

    public CommandNotFoundException(string commandName, string? message = null) : base(message ?? $"Command {commandName} not found.")
    {
        ArgumentNullException.ThrowIfNull(commandName, nameof(commandName));
        this.CommandName = commandName;
    }
}
