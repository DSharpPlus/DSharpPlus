
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IEmbedFooter" />
public sealed record EmbedFooter : IEmbedFooter
{
    /// <inheritdoc/>
    public required string Text { get; init; }

    /// <inheritdoc/>
    public Optional<string> IconUrl { get; init; }

    /// <inheritdoc/>
    public Optional<string> ProxyIconUrl { get; init; }
}