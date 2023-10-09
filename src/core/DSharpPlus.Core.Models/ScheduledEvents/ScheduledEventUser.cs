
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IScheduledEventUser" />
public sealed record ScheduledEventUser : IScheduledEventUser
{
    /// <inheritdoc/>
    public required Snowflake GuildScheduledEventId { get; init; }

    /// <inheritdoc/>
    public required IUser User { get; init; }

    /// <inheritdoc/>
    public Optional<IGuildMember> Member { get; init; }
}