// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IWebhook" />
public sealed record Webhook : IWebhook
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required DiscordWebhookType Type { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake?> GuildId { get; init; }

    /// <inheritdoc/>
    public required Snowflake? ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> User { get; init; }

    /// <inheritdoc/>
    public required string? Name { get; init; }

    /// <inheritdoc/>
    public required string? Avatar { get; init; }

    /// <inheritdoc/>
    public Optional<string> Token { get; init; }

    /// <inheritdoc/>
    public required Snowflake? ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialGuild> SourceGuild { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialChannel> SourceChannel { get; init; }

    /// <inheritdoc/>
    public Optional<string> Url { get; init; }
}