
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IEmbedAuthor" />
public sealed record EmbedAuthor : IEmbedAuthor
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<string> Url { get; init; }

    /// <inheritdoc/>
    public Optional<string> IconUrl { get; init; }

    /// <inheritdoc/>
    public Optional<string> ProxyIconUrl { get; init; }
}