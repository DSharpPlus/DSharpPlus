
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IRoleTags" />
public sealed record RoleTags : IRoleTags
{
    /// <inheritdoc/>
    public Optional<Snowflake> BotId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> IntegrationId { get; init; }

    /// <inheritdoc/>
    public required bool PremiumSubscriber { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> SubscriptionListingId { get; init; }

    /// <inheritdoc/>
    public required bool AvailableForPurchase { get; init; }

    /// <inheritdoc/>
    public required bool GuildConnections { get; init; }
}