// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/emojis/:emoji-id</c>.
/// </summary>
public interface IModifyGuildEmojiPayload
{
    /// <summary>
    /// The new name of the emoji.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The snowflake identifiers of roles allowed to use this emoji.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> Roles { get; }
}
