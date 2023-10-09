
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IDefaultSelectValue" />
public sealed record DefaultSelectValue : IDefaultSelectValue
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Type { get; init; }
}