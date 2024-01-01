// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyThreadChannelPayload" />
public sealed record ModifyThreadChannelPayload : IModifyThreadChannelPayload
{
    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Archived { get; init; }

    /// <inheritdoc/>
    public Optional<int> AutoArchiveDuration { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Locked { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Invitable { get; init; }

    /// <inheritdoc/>
    public Optional<int?> RateLimitPerUser { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordChannelFlags> Flags { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> AppliedTags { get; init; }
}