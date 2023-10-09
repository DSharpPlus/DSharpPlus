
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IFollowedChannel" />
public sealed record FollowedChannel : IFollowedChannel
{
    /// <inheritdoc/>
    public required Snowflake ChannelId { get; init; }

    /// <inheritdoc/>
    public required Snowflake WebhookId { get; init; }
}