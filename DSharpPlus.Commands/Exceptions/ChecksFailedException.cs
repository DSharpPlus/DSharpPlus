namespace DSharpPlus.Commands.Exceptions;

using System.Collections.Generic;

using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;

public sealed class ChecksFailedException : CommandsException
{
    public Command Command { get; init; }
    public IReadOnlyList<ContextCheckFailedData> Errors { get; init; }

    public ChecksFailedException(IReadOnlyList<ContextCheckFailedData> errors, Command command, string? message = null) : base(message ?? $"Checks for {command.FullName} failed.")
    {
        Command = command;
        Errors = errors;
    }
}
