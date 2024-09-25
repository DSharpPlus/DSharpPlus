using System;

namespace DSharpPlus.Commands.Converters.Results;

/// <summary>
/// This class represents an argument that failed to convert during the argument conversion process.
/// </summary>
public class ArgumentFailedConversionResult
{
    /// <summary>
    /// The exception that occurred during conversion, if any.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// The value that failed to convert.
    /// </summary>
    public object? Value { get; init; }
}
