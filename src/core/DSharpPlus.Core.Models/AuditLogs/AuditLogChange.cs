
using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IAuditLogChange" />
public sealed record AuditLogChange : IAuditLogChange
{
    /// <inheritdoc/>
    public Optional<string> NewValue { get; init; }

    /// <inheritdoc/>
    public Optional<string> OldValue { get; init; }

    /// <inheritdoc/>
    public required string Key { get; init; }
}