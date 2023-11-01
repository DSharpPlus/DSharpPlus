using System;

namespace DSharpPlus.CommandAll.Exceptions
{
    public sealed class CommandNotFoundException(string CommandName, string? message = null) : Exception(message ?? $"Command {CommandName} not found.");
}
