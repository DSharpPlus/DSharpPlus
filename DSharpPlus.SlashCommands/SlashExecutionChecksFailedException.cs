namespace DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;

/// <summary>
/// Thrown when a pre-execution check for a slash command fails.
/// </summary>
public sealed class SlashExecutionChecksFailedException : Exception
{
    /// <summary>
    /// The list of failed checks.
    /// </summary>
    public IReadOnlyList<SlashCheckBaseAttribute> FailedChecks;
}
