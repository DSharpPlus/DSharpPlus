
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IBan" />
public sealed record Ban : IBan
{
    /// <inheritdoc/>
    public string? Reason { get; init; }

    /// <inheritdoc/>
    public required IUser User { get; init; }
}