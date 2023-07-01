// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents the guild audit log.
/// </summary>
public interface IAuditLog
{
    /// <summary>
    /// The application commands referenced in the audit log.
    /// </summary>
    public IReadOnlyList<IApplicationCommand> ApplicationCommands { get; }

    /// <summary>
    /// The audit log entires, sorted from most recent to last recent.
    /// </summary>
    public IReadOnlyList<IAuditLogEntry> AuditLogEntries { get; }

    /// <summary>
    /// The auto moderation rules referenced in the audit log.
    /// </summary>
    public IReadOnlyList<IAutoModerationRule> AutoModerationRules { get; }

    /// <summary>
    /// The scheduled events referenced in the audit log.
    /// </summary>
    public IReadOnlyList<IScheduledEvent> GuildScheduledEvents { get; }

    /// <summary>
    /// The integrations referenced in the audit log.
    /// </summary>
    public IReadOnlyList<IPartialIntegration> Integrations { get; }

    /// <summary>
    /// The threads referenced in the audit log.
    /// </summary>
    public IReadOnlyList<IChannel> Threads { get; }

    /// <summary>
    /// The users referenced in the audit log.
    /// </summary>
    public IReadOnlyList<IUser> Users { get; }

    /// <summary>
    /// The webhooks referenced in the audit log.
    /// </summary>
    public IReadOnlyList<IWebhook> Webhooks { get; }
}
