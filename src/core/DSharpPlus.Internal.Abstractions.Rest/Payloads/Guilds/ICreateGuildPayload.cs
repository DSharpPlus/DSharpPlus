// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds</c>.
/// </summary>
public interface ICreateGuildPayload
{
    /// <summary>
    /// The name of the guild, from 2 to 100 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The 128x128 icon of this guild.
    /// </summary>
    public Optional<ImageData> Icon { get; }

    /// <summary>
    /// The verification level required by this guild.
    /// </summary>
    public Optional<DiscordVerificationLevel> VerificationLevel { get; }

    /// <summary>
    /// The default message notification level for this guild.
    /// </summary>
    public Optional<DiscordMessageNotificationLevel> DefaultMessageNotifications { get; }

    /// <summary>
    /// The explicit content filter level for this guild.
    /// </summary>
    public Optional<DiscordExplicitContentFilterLevel> ExplicitContentFilter { get; }

    /// <summary>
    /// The roles this guild will have.
    /// </summary>
    /// <remarks>
    /// <c>ICreateGuildPayload.Roles[0]</c> is used to configure the <c>@everyone</c> role. If you are trying
    /// to bootstrap a guild with additional roles, you can set this first role to a placeholder. <br/>
    /// The <seealso cref="IPartialRole.Id"/> field is a placeholder to allow you to reference the role
    /// elsewhere, namely when passing in default channels to <seealso cref="Channels"/> and specifying
    /// overwrites for them.
    /// </remarks>
    public Optional<IReadOnlyList<IPartialRole>> Roles { get; }

    /// <summary>
    /// The channels to create this guild with. If this is set, none of the default channels will be created.
    /// </summary>
    /// <remarks>
    /// The <seealso cref="IPartialChannel.Position"/> field is ignored. <br/>
    /// The <seealso cref="IPartialChannel.Id"/> field is a placeholder to allow creating category channels by
    /// setting the <seealso cref="IPartialChannel.ParentId"/> field to the parents' ID. Category channels must
    /// be listed before any of their children. The ID also serves for other fields to reference channels.
    /// </remarks>
    public Optional<IReadOnlyList<IPartialChannel>> Channels { get; }

    /// <summary>
    /// The identifier of the AFK voice channel, referring to a placeholder ID in <seealso cref="Channels"/>.
    /// </summary>
    public Optional<Snowflake> AfkChannelId { get; }

    /// <summary>
    /// The AFK timeout in seconds, can be set to 60, 300, 900, 1800 or 3600.
    /// </summary>
    public Optional<int> AfkTimeout { get; }

    /// <summary>
    /// The identifier of the system channel where guild notices such as welcome messages are posted, referring
    /// to a placeholder ID in <seealso cref="Channels"/>.
    /// </summary>
    public Optional<Snowflake> SystemChannelId { get; }

    /// <summary>
    /// Default flags for the system channel.
    /// </summary>
    public Optional<DiscordSystemChannelFlags> SystemChannelFlags { get; }
}
