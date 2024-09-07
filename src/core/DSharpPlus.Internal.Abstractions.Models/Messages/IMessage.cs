// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a complete message object.
/// </summary>
public interface IMessage : IPartialMessage
{
    /// <inheritdoc cref="IPartialMessage.ChannelId"/>
    public new Snowflake ChannelId { get; }

    /// <inheritdoc cref="IPartialMessage.Author"/>
    public new IUser Author { get; }

    /// <inheritdoc cref="IPartialMessage.Content"/>
    public new string Content { get; }

    /// <inheritdoc cref="IPartialMessage.Timestamp"/>
    public new DateTimeOffset Timestamp { get; }

    /// <inheritdoc cref="IPartialMessage.EditedTimestamp"/>
    public new DateTimeOffset? EditedTimestamp { get; }

    /// <inheritdoc cref="IPartialMessage.Tts"/>
    public new bool Tts { get; }

    /// <inheritdoc cref="IPartialMessage.MentionEveryone"/>
    public new bool MentionEveryone { get; }

    /// <inheritdoc cref="IPartialMessage.Mentions"/>
    public new IReadOnlyList<IUser> Mentions { get; }

    /// <inheritdoc cref="IPartialMessage.MentionRoles"/>
    public new IReadOnlyList<Snowflake> MentionRoles { get; }

    /// <inheritdoc cref="IPartialMessage.Attachments"/>
    public new IReadOnlyList<IAttachment> Attachments { get; }

    /// <inheritdoc cref="IPartialMessage.Embeds"/>
    public new IReadOnlyList<IEmbed> Embeds { get; }

    /// <inheritdoc cref="IPartialMessage.Pinned"/>
    public new bool Pinned { get; }

    /// <inheritdoc cref="IPartialMessage.Type"/>
    public new DiscordMessageType Type { get; }

    // explicit routes for partial access

    /// <inheritdoc/>
    Optional<Snowflake> IPartialMessage.ChannelId => this.ChannelId;

    /// <inheritdoc/>
    Optional<IUser> IPartialMessage.Author => new(this.Author);

    /// <inheritdoc/>
    Optional<string> IPartialMessage.Content => this.Content;

    /// <inheritdoc/>
    Optional<DateTimeOffset> IPartialMessage.Timestamp => this.Timestamp;

    /// <inheritdoc/>
    Optional<DateTimeOffset?> IPartialMessage.EditedTimestamp => this.EditedTimestamp;

    /// <inheritdoc/>
    Optional<bool> IPartialMessage.Tts => this.Tts;

    /// <inheritdoc/>
    Optional<bool> IPartialMessage.MentionEveryone => this.MentionEveryone;

    /// <inheritdoc/>
    Optional<IReadOnlyList<IUser>> IPartialMessage.Mentions => new(this.Mentions);

    /// <inheritdoc/>
    Optional<IReadOnlyList<Snowflake>> IPartialMessage.MentionRoles => new(this.MentionRoles);

    /// <inheritdoc/>
    Optional<IReadOnlyList<IAttachment>> IPartialMessage.Attachments => new(this.Attachments);

    /// <inheritdoc/>
    Optional<IReadOnlyList<IEmbed>> IPartialMessage.Embeds => new(this.Embeds);

    /// <inheritdoc/>
    Optional<bool> IPartialMessage.Pinned => this.Pinned;

    /// <inheritdoc/>
    Optional<DiscordMessageType> IPartialMessage.Type => this.Type;
}
