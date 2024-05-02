using System;

namespace DSharpPlus.Commands.ContextChecks.ParameterChecks;

/// <summary>
/// Contains information about a failed parameter check.
/// </summary>
public sealed class ParameterCheckFailedData
{
    /// <summary>
    /// Metadata for the failed parameter check.
    /// </summary>
    public required ParameterCheckAttribute ParameterCheckAttribute { get; init; }

    /// <summary>
    /// The error message returned by the check.
    /// </summary>
    public required string ErrorMessage { get; init; }

    /// <summary>
    /// If applicable, the exception thrown during executing the check.
    /// </summary>
    public Exception? Exception { get; init; }
}
