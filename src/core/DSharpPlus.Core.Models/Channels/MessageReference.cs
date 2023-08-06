// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IMessageReference" />
public sealed record MessageReference : IMessageReference
{
    /// <inheritdoc/>
    public Optional<Snowflake> MessageId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<bool> FailIfNotExists { get; init; }
}