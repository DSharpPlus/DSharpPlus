namespace DSharpPlus.Commands.Exceptions;

using System.Collections.Generic;

public sealed class ChecksFailedException(IReadOnlyList<string> errors, string commandName, string? message = null)
    : CommandsException(message ?? $"Checks for {commandName} failed.")
{
    public IReadOnlyList<string> Errors { get; init; } = errors;
}
