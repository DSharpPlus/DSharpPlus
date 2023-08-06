
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IIntegrationAccount" />
public sealed record IntegrationAccount : IIntegrationAccount
{
    /// <inheritdoc/>
    public required string Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }
}