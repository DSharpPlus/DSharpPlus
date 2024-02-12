namespace DSharpPlus.Commands.Exceptions;

using System.Collections.Generic;

using DSharpPlus.Commands.ContextChecks;

public sealed class ChecksFailedException(IReadOnlyList<(ContextCheckAttribute data, string error)> errors, string commandName, string? message = null)
    : CommandsException(message ?? $"Checks for {commandName} failed.")
{
    public IReadOnlyList<(ContextCheckAttribute data, string error)> Errors { get; init; } = errors;
}
