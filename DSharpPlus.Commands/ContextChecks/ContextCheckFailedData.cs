using System;

namespace DSharpPlus.Commands.ContextChecks;

/// <summary>
/// Represents data for when a context check fails execution.
/// </summary>
public sealed class ContextCheckFailedData
{
    public required ContextCheckAttribute ContextCheckAttribute { get; init; }
    public required string ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
}
