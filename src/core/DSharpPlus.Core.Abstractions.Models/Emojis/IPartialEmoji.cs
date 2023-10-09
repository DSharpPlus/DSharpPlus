// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partial emoji object, where any or all properties may be missing.
/// </summary>
public interface IPartialEmoji
{
    /// <summary>
    /// The snowflake identifier of this emoji, if it belongs to a guild.
    /// </summary>
    public Optional<Snowflake?> Id { get; }

    /// <summary>
    /// The name of this emoji.
    /// </summary>
    public Optional<string?> Name { get; }

    /// <summary>
    /// A list of roles allowed to use this emoji, if applicable.
    /// </summary>
    public Optional<IReadOnlyList<Snowflake>> Roles { get; }

    /// <summary>
    /// The user who created this emoji.
    /// </summary>
    public Optional<IUser> User { get; }

    /// <summary>
    /// Indicates whether this emoji requires to be wrapped in colons.
    /// </summary>
    public Optional<bool> RequireColons { get; }

    /// <summary>
    /// Indicates whether this emoji is managed by an app.
    /// </summary>
    public Optional<bool> Managed { get; }

    /// <summary>
    /// Indicates whether this emoji is an animated emoji. Animated emojis have an <c>a</c> prefix in text form.
    /// </summary>
    public Optional<bool> Animated { get; }

    /// <summary>
    /// Indicates whether this emoji is currently available for use. This may be false when losing server
    /// boosts.
    /// </summary>
    public Optional<bool> Available { get; }
}
