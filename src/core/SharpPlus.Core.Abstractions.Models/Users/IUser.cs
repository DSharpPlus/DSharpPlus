// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IUser" />
public sealed record User : IUser
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required string Username { get; init; }

    /// <inheritdoc/>
    public required string Discriminator { get; init; }

    /// <inheritdoc/>
    public required string? GlobalName { get; init; }

    /// <inheritdoc/>
    public required string? Avatar { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Bot { get; init; }

    /// <inheritdoc/>
    public Optional<bool> System { get; init; }

    /// <inheritdoc/>
    public Optional<bool> MfaEnabled { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Banner { get; init; }

    /// <inheritdoc/>
    public Optional<int?> AccentColor { get; init; }

    /// <inheritdoc/>
    public Optional<string> Locale { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Verified { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Email { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordUserFlags> Flags { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPremiumType> PremiumType { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordUserFlags> PublicFlags { get; init; }

    /// <inheritdoc/>
    public Optional<string?> AvatarDecoration { get; init; }
}