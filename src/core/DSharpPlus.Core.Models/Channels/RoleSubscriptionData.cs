
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IRoleSubscriptionData" />
public sealed record RoleSubscriptionData : IRoleSubscriptionData
{
    /// <inheritdoc/>
    public required Snowflake RoleSubscriptionListingId { get; init; }

    /// <inheritdoc/>
    public required string TierName { get; init; }

    /// <inheritdoc/>
    public required int TotalMonthsSubscribed { get; init; }

    /// <inheritdoc/>
    public required bool IsRenewal { get; init; }
}