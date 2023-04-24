// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Gets message in a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    public Task<DiscordMessage> GetMessageAsync(ulong channelId, ulong messageId)
        => ApiClient.GetMessageAsync(channelId, messageId);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">Message (text) content</param>
    public Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content) => ApiClient.CreateMessageAsync
    (
        channelId,
        content,
        null,
        replyMessageId: null,
        mentionReply: false,
        failOnInvalidReply: false,
        suppressNotifications: false
    );

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="embed">Embed to attach</param>
    public Task<DiscordMessage> CreateMessageAsync(ulong channelId, DiscordEmbed embed) => ApiClient.CreateMessageAsync
    (
        channelId,
        null,
        embed != null ? new[] { embed } : null,
        replyMessageId: null,
        mentionReply: false,
        failOnInvalidReply: false,
        suppressNotifications: false
    );

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="content">Message (text) content</param>
    /// <param name="embed">Embed to attach</param>
    public Task<DiscordMessage> CreateMessageAsync(ulong channelId, string content, DiscordEmbed embed) => ApiClient.CreateMessageAsync
    (
        channelId,
        content,
        embed != null ? new[] { embed } : null,
        replyMessageId: null,
        mentionReply: false,
        failOnInvalidReply: false,
        suppressNotifications: false
    );

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="builder">The Discord Message builder.</param>
    public Task<DiscordMessage> CreateMessageAsync(ulong channelId, DiscordMessageBuilder builder)
        => ApiClient.CreateMessageAsync(channelId, builder);

    /// <summary>
    /// Sends a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="action">The Discord Message builder.</param>
    public Task<DiscordMessage> CreateMessageAsync(ulong channelId, Action<DiscordMessageBuilder> action)
    {
        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
        action(messageBuilder);
        return ApiClient.CreateMessageAsync(channelId, messageBuilder);
    }

    /// <summary>
    /// Gets messages from a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="limit">Limit of messages to get</param>
    /// <param name="before">Gets messages before this ID</param>
    /// <param name="after">Gets messages after this ID</param>
    /// <param name="around">Gets messages around this ID</param>
    public Task<IReadOnlyList<DiscordMessage>> GetChannelMessagesAsync(ulong channelId, int limit, ulong? before, ulong? after, ulong? around)
        => ApiClient.GetChannelMessagesAsync(channelId, limit, before, after, around);

    /// <summary>
    /// Gets a message from a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    public Task<DiscordMessage> GetChannelMessageAsync(ulong channelId, ulong messageId)
        => ApiClient.GetChannelMessageAsync(channelId, messageId);

    /// <summary>
    /// Edits a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="content">New message content</param>
    public Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<string> content) => ApiClient.EditMessageAsync
    (
        channelId,
        messageId,
        content,
        default,
        default,
        default,
        Array.Empty<DiscordMessageFile>(),
        null,
        default
    );

    /// <summary>
    /// Edits a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="embed">New message embed</param>
    public Task<DiscordMessage> EditMessageAsync(ulong channelId, ulong messageId, Optional<DiscordEmbed> embed) => ApiClient.EditMessageAsync
    (
        channelId,
        messageId,
        default,
        embed.HasValue ? new[] { embed.Value } : Array.Empty<DiscordEmbed>(),
        default,
        default,
        Array.Empty<DiscordMessageFile>(),
        null,
        default
    );

    /// <summary>
    /// Edits a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="builder">The builder of the message to edit.</param>
    /// <param name="suppressEmbeds">Whether to suppress embeds on the message.</param>
    /// <param name="attachments">Attached files to keep.</param>
    public Task<DiscordMessage> EditMessageAsync
    (
        ulong channelId,
        ulong messageId,
        DiscordMessageBuilder builder,
        bool suppressEmbeds = false,
        IEnumerable<DiscordAttachment>? attachments = null
    )
    {
        builder.Validate(true);

        return ApiClient.EditMessageAsync(
            channelId,
            messageId,
            builder.Content,
            new Optional<IEnumerable<DiscordEmbed>>(builder.Embeds),
            builder._mentions,
            builder.Components,
            builder.Files,
            suppressEmbeds ? MessageFlags.SuppressedEmbeds : null,
            attachments
        );
    }

    /// <summary>
    /// Modifies the visibility of embeds in a message.
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="hideEmbeds">Whether to hide all embeds.</param>
    public Task ModifyEmbedSuppressionAsync(ulong channelId, ulong messageId, bool hideEmbeds) => ApiClient.EditMessageAsync
    (
        channelId,
        messageId,
        default,
        default,
        default,
        default,
        Array.Empty<DiscordMessageFile>(),
        hideEmbeds ? MessageFlags.SuppressedEmbeds : null,
        default
    );

    /// <summary>
    /// Deletes a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="reason">Why this message was deleted</param>
    public Task DeleteMessageAsync(ulong channelId, ulong messageId, string reason)
        => ApiClient.DeleteMessageAsync(channelId, messageId, reason);

    /// <summary>
    /// Deletes multiple messages
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageIds">Message IDs</param>
    /// <param name="reason">Reason these messages were deleted</param>
    public Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds, string reason)
        => ApiClient.DeleteMessagesAsync(channelId, messageIds, reason);

    /// <summary>
    /// Send a typing indicator to a channel
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    public Task TriggerTypingAsync(ulong channelId)
        => ApiClient.TriggerTypingAsync(channelId);

    /// <summary>
    /// Gets pinned messages
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessagesAsync(ulong channelId)
        => ApiClient.GetPinnedMessagesAsync(channelId);

    /// <summary>
    /// Unpins a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    public Task UnpinMessageAsync(ulong channelId, ulong messageId)
        => ApiClient.UnpinMessageAsync(channelId, messageId);

    /// <summary>
    /// Publishes a message in a news channel to following channels
    /// </summary>
    /// <param name="channelId">ID of the news channel the message to crosspost belongs to</param>
    /// <param name="messageId">ID of the message to crosspost</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when the current user doesn't have <see cref="Permissions.ManageWebhooks"/> and/or <see cref="Permissions.SendMessages"/></exception>
    public Task<DiscordMessage> CrosspostMessageAsync(ulong channelId, ulong messageId)
        => ApiClient.CrosspostMessageAsync(channelId, messageId);

    /// <summary>
    /// Pins a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel the message is in.</param>
    /// <param name="messageId">The ID of the message.</param>
    public Task PinMessageAsync(ulong channelId, ulong messageId)
        => ApiClient.PinMessageAsync(channelId, messageId);
}
