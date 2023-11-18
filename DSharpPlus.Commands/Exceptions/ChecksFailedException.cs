namespace DSharpPlus.Commands.Exceptions;

using System;
using System.Collections.Generic;

public sealed class ChecksFailedException
(
    IReadOnlyList<Exception> errors,
    string commandName,
    string? message = null
) 
: Exception
(
    message ?? $"Checks for {commandName} failed."
)
{
    public IReadOnlyList<Exception> Check { get; init; } = errors;
}
