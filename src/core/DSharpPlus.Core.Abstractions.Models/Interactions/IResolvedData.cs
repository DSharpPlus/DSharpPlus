// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents data resolved by Discord from <seealso cref="IInteraction"/>s.
/// </summary>
public interface IResolvedData
{
    /// <summary>
    /// Maps snowflakes to resolved user objects.
    /// </summary>
    public Optional<IReadOnlyDictionary<Snowflake, IUser>> Users { get; }

    /// <summary>
    /// Maps snowflakes to resolved guild member objects.
    /// </summary>
    public Optional<IReadOnlyDictionary<Snowflake, IPartialGuildMember>> Members { get; }

    /// <summary>
    /// Maps snowflakes to role objects.
    /// </summary>
    public Optional<IReadOnlyDictionary<Snowflake, IRole>> Roles { get; }

    /// <summary>
    /// Maps snowflakes to channel objects.
    /// </summary>
    public Optional<IReadOnlyDictionary<Snowflake, IPartialChannel>> Channels { get; }

    /// <summary>
    /// Maps snowflakes to message objects.
    /// </summary>
    public Optional<IReadOnlyDictionary<Snowflake, IPartialMessage>> Messages { get; }

    /// <summary>
    /// Maps snowflakes to attachment objects.
    /// </summary>
    public Optional<IReadOnlyDictionary<Snowflake, IAttachment>> Attachments { get; }
}
