
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IReaction" />
public sealed record Reaction : IReaction
{
    /// <inheritdoc/>
    public required int Count { get; init; }

    /// <inheritdoc/>
    public required bool Me { get; init; }

    /// <inheritdoc/>
    public required IPartialEmoji Emoji { get; init; }
}