
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IVoiceRegion" />
public sealed record VoiceRegion : IVoiceRegion
{
    /// <inheritdoc/>
    public required string Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required bool Optimal { get; init; }

    /// <inheritdoc/>
    public required bool Deprecated { get; init; }

    /// <inheritdoc/>
    public required bool Custom { get; init; }
}