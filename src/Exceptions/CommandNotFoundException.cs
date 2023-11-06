using System;

namespace DSharpPlus.CommandAll.Exceptions
{
    public sealed class CommandNotFoundException : Exception
    {
        public string CommandName { get; init; }

        public CommandNotFoundException(string commandName, string? message = null) : base(message ?? $"Command {commandName} not found.")
        {
            ArgumentNullException.ThrowIfNull(commandName, nameof(commandName));
            CommandName = commandName;
        }
    }
}
