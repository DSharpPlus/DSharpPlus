// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

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