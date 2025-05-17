using System.Collections.Generic;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Exceptions;

public sealed class ChecksFailedException : CommandsException
{
    public Command Command { get; init; }
    public IReadOnlyList<ContextCheckFailedData> Errors { get; init; }

    public ChecksFailedException(IReadOnlyList<ContextCheckFailedData> errors, Command command, string? message = null) : base(message ?? $"Checks for {command.FullName} failed.")
    {
        this.Command = command;
        this.Errors = errors;
    }
}
