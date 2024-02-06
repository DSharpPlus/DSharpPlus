namespace DSharpPlus.Commands.Exceptions;

using System;
using System.Collections.Generic;
using DSharpPlus.Commands.ContextChecks;

public sealed class ChecksFailedException(IReadOnlyDictionary<ContextCheckAttribute, Exception> errors, string commandName, string? message = null)
    : CommandsException(message ?? $"Checks for {commandName} failed.")
{
    public IReadOnlyDictionary<ContextCheckAttribute, Exception> FailingContextChecks { get; init; } = errors;
}
