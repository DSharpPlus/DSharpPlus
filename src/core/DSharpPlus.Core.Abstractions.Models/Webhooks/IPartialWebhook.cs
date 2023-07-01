// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a partially populated webhook object.
/// </summary>
public interface IPartialWebhook
{
    /// <summary>
    /// The snowflake identifier of this webhook.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The type of this webhook.
    /// </summary>
    public Optional<DiscordWebhookType> Type { get; }

    /// <summary>
    /// The snowflake identifier of the guild this webhook is for, if any.
    /// </summary>
    public Optional<Snowflake?> GuildId { get; }

    /// <summary>
    /// The snowflake identifier of the channel this webhook is for, if any.
    /// </summary>
    public Optional<Snowflake?> ChannelId { get; }

    /// <summary>
    /// The user who created this webhook.
    /// </summary>
    public Optional<IUser> User { get; }

    /// <summary>
    /// The default name of this webhook.
    /// </summary>
    public Optional<string?> Name { get; }

    /// <summary>
    /// The default avatar hash of this webhook.
    /// </summary>
    public Optional<string?> Avatar { get; }

    /// <summary>
    /// The secure token of this webhook.
    /// </summary>
    public Optional<string> Token { get; }

    /// <summary>
    /// The snowflake identifier of the bot/oauth2 application which created this webhook.
    /// </summary>
    public Optional<Snowflake?> ApplicationId { get; }

    /// <summary>
    /// The guild containing the channel that this webhook is following.
    /// </summary>
    public Optional<IPartialGuild> SourceGuild { get; }

    /// <summary>
    /// The channel that this webhook is following.
    /// </summary>
    public Optional<IPartialChannel> SourceChannel { get; }

    /// <summary>
    /// The url used to execute this webhook.
    /// </summary>
    public Optional<string> Url { get; }
}
