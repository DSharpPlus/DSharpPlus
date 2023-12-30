// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a discord webhook.
/// </summary>
public interface IWebhook : IPartialWebhook
{
    /// <inheritdoc cref="IPartialWebhook.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialWebhook.Type"/>
    public new DiscordWebhookType Type { get; }

    /// <inheritdoc cref="IPartialWebhook.ChannelId"/>
    public new Snowflake? ChannelId { get; }

    /// <inheritdoc cref="IPartialWebhook.Name"/>
    public new string? Name { get; }

    /// <inheritdoc cref="IPartialWebhook.Avatar"/>
    public new string? Avatar { get; }

    /// <inheritdoc cref="IPartialWebhook.ApplicationId"/>
    public new Snowflake? ApplicationId { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialWebhook.Id => this.Id;

    /// <inheritdoc/>
    Optional<DiscordWebhookType> IPartialWebhook.Type => this.Type;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialWebhook.ChannelId => this.ChannelId;

    /// <inheritdoc/>
    Optional<string?> IPartialWebhook.Name => this.Name;

    /// <inheritdoc/>
    Optional<string?> IPartialWebhook.Avatar => this.Avatar;

    /// <inheritdoc/>
    Optional<Snowflake?> IPartialWebhook.ApplicationId => this.ApplicationId;
}
