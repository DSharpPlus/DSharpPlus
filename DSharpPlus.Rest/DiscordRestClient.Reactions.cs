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

using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus;

public sealed partial class DiscordRestClient
{
    /// <summary>
    /// Creates a new reaction
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to react</param>
    public Task CreateReactionAsync(ulong channelId, ulong messageId, string emoji)
        => ApiClient.CreateReactionAsync(channelId, messageId, emoji);

    /// <summary>
    /// Deletes own reaction
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to remove from reaction</param>
    public Task DeleteOwnReactionAsync(ulong channelId, ulong messageId, string emoji)
        => ApiClient.DeleteOwnReactionAsync(channelId, messageId, emoji);

    /// <summary>
    /// Deletes someone elses reaction
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="emoji">Emoji to remove</param>
    /// <param name="reason">Reason why this reaction was removed</param>
    public Task DeleteUserReactionAsync(ulong channelId, ulong messageId, ulong userId, string emoji, string reason)
        => ApiClient.DeleteUserReactionAsync(channelId, messageId, userId, emoji, reason);

    /// <summary>
    /// Gets all users that reacted with a specific emoji to a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to check for</param>
    /// <param name="afterId">Whether to search for reactions after this message id.</param>
    /// <param name="limit">The maximum amount of reactions to fetch.</param>
    public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, string emoji, ulong? afterId = null, int limit = 25)
        => ApiClient.GetReactionsAsync(channelId, messageId, emoji, afterId, limit);

    /// <summary>
    /// Gets all users that reacted with a specific emoji to a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="emoji">Emoji to check for</param>
    /// <param name="afterId">Whether to search for reactions after this message id.</param>
    /// <param name="limit">The maximum amount of reactions to fetch.</param>
    public Task<IReadOnlyList<DiscordUser>> GetReactionsAsync(ulong channelId, ulong messageId, DiscordEmoji emoji, ulong? afterId = null, int limit = 25)
        => ApiClient.GetReactionsAsync(channelId, messageId, emoji.ToReactionString(), afterId, limit);

    /// <summary>
    /// Deletes all reactions from a message
    /// </summary>
    /// <param name="channelId">Channel ID</param>
    /// <param name="messageId">Message ID</param>
    /// <param name="reason">Reason why all reactions were removed</param>
    public Task DeleteAllReactionsAsync(ulong channelId, ulong messageId, string reason)
        => ApiClient.DeleteAllReactionsAsync(channelId, messageId, reason);

    /// <summary>
    /// Deletes all reactions of a specific reaction for a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="messageId">The ID of the message.</param>
    /// <param name="emoji">The emoji to clear.</param>
    public Task DeleteReactionsEmojiAsync(ulong channelId, ulong messageId, string emoji)
        => ApiClient.DeleteReactionsEmojiAsync(channelId, messageId, emoji);
}
