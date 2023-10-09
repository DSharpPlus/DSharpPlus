
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IEmbedField" />
public sealed record EmbedField : IEmbedField
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required string Value { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Inline { get; init; }
}