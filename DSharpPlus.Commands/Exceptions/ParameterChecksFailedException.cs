namespace DSharpPlus.Commands.Exceptions;

using System.Collections.Generic;

using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Commands.Trees;

public class ParameterChecksFailedException : CommandsException
{
    public Command Command { get; init; }
    public IReadOnlyList<ParameterCheckFailedData> Errors { get; init; }

    public ParameterChecksFailedException(IReadOnlyList<ParameterCheckFailedData> errors, Command command, string? message = null) : base(message ?? $"Checks for {command.FullName} failed.")
    {
        Command = command;
        Errors = errors;
    }
}
