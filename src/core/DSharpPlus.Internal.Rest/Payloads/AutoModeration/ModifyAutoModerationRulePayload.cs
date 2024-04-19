// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyAutoModerationRulePayload" />
public sealed record ModifyAutoModerationRulePayload : IModifyAutoModerationRulePayload
{
    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordAutoModerationEventType> EventType { get; init; }

    /// <inheritdoc/>
    public Optional<IAutoModerationTriggerMetadata> TriggerMetadata { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IAutoModerationAction>> Actions { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Enabled { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> ExemptRoles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> ExemptChannels { get; init; }
}
