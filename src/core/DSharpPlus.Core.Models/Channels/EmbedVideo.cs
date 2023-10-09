
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IEmbedVideo" />
public sealed record EmbedVideo : IEmbedVideo
{
    /// <inheritdoc/>
    public Optional<string> Url { get; init; }

    /// <inheritdoc/>
    public Optional<string> ProxyUrl { get; init; }

    /// <inheritdoc/>
    public Optional<int> Height { get; init; }

    /// <inheritdoc/>
    public Optional<int> Width { get; init; }
}