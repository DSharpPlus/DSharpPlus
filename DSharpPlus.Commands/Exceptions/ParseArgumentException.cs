namespace DSharpPlus.Commands.Exceptions;

using System;
using DSharpPlus.Commands.Trees;

/// <summary>
/// Thrown when an argument could not be parsed.
/// </summary>
public sealed class ArgumentParseException : CommandsException
{
    /// <summary>
    /// The parameter which failed to parse properly
    /// </summary>
    public CommandParameter Parameter { get; init; }

    /// <summary>
    /// Thrown when an argument could not be parsed.
    /// </summary>
    /// <param name="parameter">The parameter which failed to parse properly</param>
    /// <param name="innerException">Gets the Exception instance that caused the current exception.</param>
    /// <param name="message">Custom exception message.</param>
    public ArgumentParseException(CommandParameter parameter, Exception? innerException = null, string? message = null) : base(message ?? $"Failed to parse {parameter.Name}.", innerException)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));
        this.Parameter = parameter;
    }
}
