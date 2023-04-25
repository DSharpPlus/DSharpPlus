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
    /// Creates a thread from a message.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="messageId">The ID of the message </param>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration.</param>
    /// <param name="reason">Reason for audit logs.</param>
    public Task<DiscordThreadChannel> CreateThreadFromMessageAsync(ulong channelId, ulong messageId, string name, AutoArchiveDuration archiveAfter, string? reason = null)
       => ApiClient.CreateThreadFromMessageAsync(channelId, messageId, name, archiveAfter, reason);

    /// <summary>
    /// Creates a thread.
    /// </summary>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="name">The name of the thread.</param>
    /// <param name="archiveAfter">The auto archive duration.</param>
    /// <param name="threadType">The type of the thread.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    public Task<DiscordThreadChannel> CreateThreadAsync(ulong channelId, string name, AutoArchiveDuration archiveAfter, ChannelType threadType, string? reason = null)
       => ApiClient.CreateThreadAsync(channelId, name, archiveAfter, threadType, reason);

    /// <summary>
    /// Joins a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    public Task JoinThreadAsync(ulong threadId)
        => ApiClient.JoinThreadAsync(threadId);

    /// <summary>
    /// Leaves a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    public Task LeaveThreadAsync(ulong threadId)
        => ApiClient.LeaveThreadAsync(threadId);

    /// <summary>
    /// Adds a member to a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    /// <param name="userId">The ID of the member.</param>
    public Task AddThreadMemberAsync(ulong threadId, ulong userId)
        => ApiClient.AddThreadMemberAsync(threadId, userId);

    /// <summary>
    /// Removes a member from a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    /// <param name="userId">The ID of the member.</param>
    public Task RemoveThreadMemberAsync(ulong threadId, ulong userId)
        => ApiClient.RemoveThreadMemberAsync(threadId, userId);

    /// <summary>
    /// Lists the members of a thread.
    /// </summary>
    /// <param name="threadId">The ID of the thread.</param>
    public Task<IReadOnlyList<DiscordThreadChannelMember>> ListThreadMembersAsync(ulong threadId)
        => ApiClient.ListThreadMembersAsync(threadId);

    /// <summary>
    /// Lists the active threads of a guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    public Task<ThreadQueryResult> ListActiveThreadAsync(ulong guildId)
        => ApiClient.ListActiveThreadsAsync(guildId);

    /// <summary>
    /// Gets the threads that are public and archived for a channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="before">Date to filter by.</param>
    /// <param name="limit">How many threads to return.</param>
    public Task<ThreadQueryResult> ListPublicArchivedThreadsAsync(ulong guildId, ulong channelId, DateTimeOffset? before = null, int limit = 0)
       => ApiClient.ListPublicArchivedThreadsAsync(guildId, channelId, before?.ToString("o"), limit);

    /// <summary>
    /// Gets the threads that are public and archived for a channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="before">Date to filter by.</param>
    /// <param name="limit">How many threads to return.</param>
    public Task<ThreadQueryResult> ListPrivateArchivedThreadAsync(ulong guildId, ulong channelId, DateTimeOffset? before = null, int limit = 0)
       => ApiClient.ListPrivateArchivedThreadsAsync(guildId, channelId, before?.ToString("o"), limit);

    /// <summary>
    /// Gets the private archived threads the user has joined for a channel.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <param name="channelId">The ID of the channel.</param>
    /// <param name="before">Date to filter by.</param>
    /// <param name="limit">How many threads to return.</param>
    public Task<ThreadQueryResult> ListJoinedPrivateArchivedThreadsAsync(ulong guildId, ulong channelId, DateTimeOffset? before = null, int limit = 0)
       => ApiClient.ListJoinedPrivateArchivedThreadsAsync(guildId, channelId, (ulong?)before?.ToUnixTimeSeconds(), limit);
}
