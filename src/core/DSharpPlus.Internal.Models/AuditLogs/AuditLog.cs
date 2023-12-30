// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IAuditLog" />
public sealed record AuditLog : IAuditLog
{
    /// <inheritdoc/>
    public required IReadOnlyList<IApplicationCommand> ApplicationCommands { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IAuditLogEntry> AuditLogEntries { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IAutoModerationRule> AutoModerationRules { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IScheduledEvent> GuildScheduledEvents { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IPartialIntegration> Integrations { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IChannel> Threads { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IUser> Users { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IWebhook> Webhooks { get; init; }
}