// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IResolvedData" />
public sealed record ResolvedData : IResolvedData
{
    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<Snowflake, IUser>> Users { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<Snowflake, IPartialGuildMember>> Members { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<Snowflake, IRole>> Roles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<Snowflake, IPartialChannel>> Channels { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<Snowflake, IPartialMessage>> Messages { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyDictionary<Snowflake, IAttachment>> Attachments { get; init; }
}