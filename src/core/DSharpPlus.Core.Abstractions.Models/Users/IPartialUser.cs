// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partially populated user object.
/// </summary>
public interface IPartialUser
{
    /// <summary>
    /// The snowflake identifier of this user.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The username of this user, unique across the platform.
    /// </summary>
    public Optional<string> Username { get; }

    /// <summary>
    /// The global display name of this user.
    /// </summary>
    public Optional<string?> GlobalName { get; }

    /// <summary>
    /// The user's avatar hash.
    /// </summary>
    public Optional<string?> Avatar { get; }

    /// <summary>
    /// Indicates whether this user is a bot user.
    /// </summary>
    public Optional<bool> Bot { get; }

    /// <summary>
    /// Indicates whether this user is part of Discords urgent message system.
    /// </summary>
    public Optional<bool> System { get; }

    /// <summary>
    /// Indicates whether this user has multi-factor authentication enabled on their account.
    /// </summary>
    public Optional<bool> MfaEnabled { get; }

    /// <summary>
    /// The user's banner hash.
    /// </summary>
    public Optional<string?> Banner { get; }

    /// <summary>
    /// The user's banner color code.
    /// </summary>
    public Optional<int?> AccentColor { get; }

    /// <summary>
    /// The user's chosen language option.
    /// </summary>
    public Optional<string> Locale { get; }

    /// <summary>
    /// Indicates whether the email address linked to this user account has been verified.
    /// </summary>
    public Optional<bool> Verified { get; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public Optional<string?> Email { get; }

    /// <summary>
    /// The flags on this user's account.
    /// </summary>
    public Optional<DiscordUserFlags> Flags { get; }

    /// <summary>
    /// The level of nitro subscription on this user's account.
    /// </summary>
    public Optional<DiscordPremiumType> PremiumType { get; }

    /// <summary>
    /// The publicly visible flags on this user's account.
    /// </summary>
    public Optional<DiscordUserFlags> PublicFlags { get; }
}
