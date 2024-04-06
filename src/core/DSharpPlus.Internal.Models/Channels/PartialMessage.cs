// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IPartialMessage" />
public sealed record PartialMessage : IPartialMessage
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ChannelId { get; init; }

    /// <inheritdoc/>
    public Optional<IUser> Author { get; init; }

    /// <inheritdoc/>
    public Optional<string> Content { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset> Timestamp { get; init; }

    /// <inheritdoc/>
    public Optional<DateTimeOffset?> EditedTimestamp { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Tts { get; init; }

    /// <inheritdoc/>
    public Optional<bool> MentionEveryone { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IUser>> Mentions { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> MentionRoles { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IChannelMention>> MentionChannels { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IAttachment>> Attachments { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IEmbed>> Embeds { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IReaction>> Reactions { get; init; }

    /// <inheritdoc/>
    public Optional<object> Nonce { get; init; }

    /// <inheritdoc/>
    public Optional<bool> Pinned { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> WebhookId { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMessageType> Type { get; init; }

    /// <inheritdoc/>
    public Optional<IMessageActivity> Activity { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialApplication> Application { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> ApplicationId { get; init; }

    /// <inheritdoc/>
    public Optional<IMessageReference> MessageReference { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordMessageFlags> Flags { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialMessage?> ReferencedMessage { get; init; }

    /// <inheritdoc/>
    public Optional<IMessageInteractionMetadata> InteractionMetadata { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialChannel> Thread { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IActionRowComponent>> Components { get; init; }

    /// <inheritdoc/>
    public Optional<IStickerItem> StickerItems { get; init; }

    /// <inheritdoc/>
    public Optional<int> Position { get; init; }

    /// <inheritdoc/>
    public Optional<IRoleSubscriptionData> RoleSubscriptionData { get; init; }
}
