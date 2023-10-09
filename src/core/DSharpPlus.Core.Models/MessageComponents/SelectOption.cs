
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="ISelectOption" />
public sealed record SelectOption : ISelectOption
{
    /// <inheritdoc/>
    public required string Label { get; init; }

    /// <inheritdoc/>
    public required string Value { get; init; }

    /// <inheritdoc/>
    public Optional<string> Description { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialEmoji> Emoji { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Default { get; init; }
}