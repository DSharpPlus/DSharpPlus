
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IGuildWidgetSettings" />
public sealed record GuildWidgetSettings : IGuildWidgetSettings
{
    /// <inheritdoc/>
    public required bool Enabled { get; init; }

    /// <inheritdoc/>
    public Snowflake? ChannelId { get; init; }
}