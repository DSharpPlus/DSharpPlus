namespace DSharpPlus.Commands.Exceptions;

using System;
using System.Collections.Generic;

/// <summary>
/// Indicates that one or more checks for given command have failed.
/// <param name="errors">The list of errors thrown while running pre-execution checks</param>
/// <param name="commandName">Name of the command</param>
/// <param name="message">Custom exception message</param>
/// </summary>
public sealed class ChecksFailedException(IReadOnlyList<Exception> errors, string commandName, string? message = null)
    : CommandsException(message ?? $"Checks for {commandName} failed.")
{
    /// <summary>
    /// The list of errors thrown while running pre-execution checks.
    /// </summary>
    public IReadOnlyList<Exception> Check { get; init; } = errors;
}
