using System;
using DSharpPlus.Commands.Converters.Results;
using DSharpPlus.Commands.Trees;

namespace DSharpPlus.Commands.Exceptions;

/// <summary>
/// Indicates that an argument failed to parse.
/// </summary>
public sealed class ArgumentParseException : CommandsException
{
    /// <summary>
    /// The argument failed to parse
    /// </summary>
    public CommandParameter Parameter { get; init; }

    /// <summary>
    /// The result of the conversion, containing the exception and possibly the failed value.
    /// </summary>
    public ArgumentFailedConversionResult? ConversionResult { get; init; }

    /// <summary>
    /// Creates a new argument parse exception.
    /// </summary>
    /// <param name="parameter">The parameter that failed to parse.</param>
    /// <param name="conversionResult">The result of the conversion, containing the exception and possibly the failed value.</param>
    /// <param name="message">The message to display.</param>
    public ArgumentParseException(CommandParameter parameter, ArgumentFailedConversionResult? conversionResult = null, string? message = null)
        : base(message ?? $"Failed to parse {parameter.Name}.", conversionResult?.Error)
    {
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));
        this.Parameter = parameter;
        this.ConversionResult = conversionResult;
    }
}
