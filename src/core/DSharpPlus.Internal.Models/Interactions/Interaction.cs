// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

using OneOf;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IInteraction" />
public sealed record Interaction : IInteraction
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required Snowflake ApplicationId { get; init; }

    /// <inheritdoc/>
    public required DiscordInteractionType Type { get; init; }

    /// <inheritdoc/>
    public Optional<OneOf<IApplicationCommandInteractionData, IMessageComponentInteractionData, IModalInteractionData>> Data { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialChannel> Channel { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<IGuildMember> Member { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public required string Token { get; init; }

    /// <inheritdoc/>
    public required int Version { get; init; }

    /// <inheritdoc/>
    public Optional<IMessage> Message { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordPermissions> AppPermissions { get; init; }

    /// <inheritdoc/>
    public Optional<string> Locale { get; init; }

    /// <inheritdoc/>
    public Optional<string> GuildLocale { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IEntitlement>> Entitlements { get; init; }
}