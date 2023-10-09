// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Stores information about what mentions should be allowed and which ones should be
/// ignored when handling read states and notifications. Refer to 
/// <seealso href="https://discord.com/developers/docs/resources/channel#allowed-mentions-object-allowed-mentions-structure">
/// the Discord docs</seealso> for further information.
/// </summary>
public interface IAllowedMentions
{
    /// <summary>
    /// An array of allowed mention types to parse from the message content. This may contain
    /// "roles" for parsing role mentions, "users" for parsing user mentions and 
    /// "everyone" for parsing @everyone and @here mentions.
    /// </summary>
    public IReadOnlyList<string> Parse { get; }

    /// <summary>
    /// An array of role IDs to mention, up to 100.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> Roles { get; }

    /// <summary>
    /// An array of user IDs to mention, up to 100.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> Users { get; }

    /// <summary>
    /// For replies, this controls whether to mention the author of the replied message.
    /// </summary>
    public Optional<bool> RepliedUser { get; }
}
