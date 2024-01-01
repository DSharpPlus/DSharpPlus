// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialWebhook" />
public sealed record PartialWebhook : IPartialWebhook
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordWebhookType> Type { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Name { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Avatar { get; init; }

    /// <inheritdoc/>
    public Optional<string> Token { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialGuild> SourceGuild { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialChannel> SourceChannel { get; init; }

    /// <inheritdoc/>
    public Optional<string> Url { get; init; }
}