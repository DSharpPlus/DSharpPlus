
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IMessageReference" />
public sealed record MessageReference : IMessageReference
{
    /// <inheritdoc/>
    public Optional<Snowflake> MessageId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<bool> FailIfNotExists { get; init; }
}