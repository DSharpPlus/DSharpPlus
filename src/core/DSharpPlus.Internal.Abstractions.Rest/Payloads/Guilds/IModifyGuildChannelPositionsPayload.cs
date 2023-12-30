// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /guilds/:guild-id/channels</c>.
/// </summary>
public interface IModifyGuildChannelPositionsPayload
{
    /// <summary>
    /// The snowflake identifier of the channel to be moved.
    /// </summary>
    public Snowflake ChannelId { get; }

    /// <summary>
    /// The new sorting position for this channel.
    /// </summary>
    public Optional<int?> Position { get; }

    /// <summary>
    /// Whether this channel should sync permissions with its new parent, if moving to a new parent category.
    /// </summary>
    public Optional<bool?> LockPermissions { get; }

    /// <summary>
    /// Snowflake identifier of this channels new parent channel.
    /// </summary>
    public Optional<Snowflake?> ParentChannelId { get; }
}
