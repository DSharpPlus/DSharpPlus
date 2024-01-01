// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyGuildMemberPayload" />
public sealed record ModifyGuildMemberPayload : IModifyGuildMemberPayload
{
    /// <inheritdoc/>
    public Optional<string?> Nickname { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>?> Roles { get; init; }

    /// <inheritdoc/>
    public Optional<bool?> Mute { get; init; }

    /// <inheritdoc/>
    public Optional<bool?> Deaf { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> CommunicationDisabledUntil { get; init; }
}