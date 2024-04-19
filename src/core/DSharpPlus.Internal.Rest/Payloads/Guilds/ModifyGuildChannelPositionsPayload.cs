// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyGuildChannelPositionsPayload" />
public sealed record ModifyGuildChannelPositionsPayload : IModifyGuildChannelPositionsPayload
{
    /// <inheritdoc/>
    public required Snowflake ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<int?> Position { get; init; }

    /// <inheritdoc/>
    public Optional<bool?> LockPermissions { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> ParentChannelId { get; init; }
}
