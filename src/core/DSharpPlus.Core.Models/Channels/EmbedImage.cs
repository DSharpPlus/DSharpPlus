
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IEmbedImage" />
public sealed record EmbedImage : IEmbedImage
{
    /// <inheritdoc/>
    public required string Url { get; init; }

    /// <inheritdoc/>
    public Optional<string> ProxyUrl { get; init; }

    /// <inheritdoc/>
    public Optional<int> Height { get; init; }

    /// <inheritdoc/>
    public Optional<int> Width { get; init; }
}