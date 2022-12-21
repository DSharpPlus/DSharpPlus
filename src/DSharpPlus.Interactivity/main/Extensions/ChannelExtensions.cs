// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

namespace DSharpPlus.Interactivity.Extensions;

/// <summary>
/// Interactivity extension methods for <see cref="DiscordChannel"/>.
/// </summary>
public static class ChannelExtensions
{
    /// <summary>
    /// Waits for the next message sent in this channel that satisfies the predicate.
    /// </summary>
    /// <param name="channel">The channel to monitor.</param>
    /// <param name="predicate">A predicate that should return <see langword="true"/> if a message matches.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
    public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, Func<DiscordMessage, bool> predicate, TimeSpan? timeoutOverride = null)
        => GetInteractivity(channel).WaitForMessageAsync(msg => msg.ChannelId == channel.Id && predicate(msg), timeoutOverride);

    /// <summary>
    /// Waits for the next message sent in this channel.
    /// </summary>
    /// <param name="channel">The channel to monitor.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
    public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, TimeSpan? timeoutOverride = null)
        => channel.GetNextMessageAsync(msg => true, timeoutOverride);

    /// <summary>
    /// Waits for the next message sent in this channel from a specific user.
    /// </summary>
    /// <param name="channel">The channel to monitor.</param>
    /// <param name="user">The target user.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
    public static Task<InteractivityResult<DiscordMessage>> GetNextMessageAsync(this DiscordChannel channel, DiscordUser user, TimeSpan? timeoutOverride = null)
        => channel.GetNextMessageAsync(msg => msg.Author.Id == user.Id, timeoutOverride);

    /// <summary>
    /// Waits for a specific user to start typing in this channel.
    /// </summary>
    /// <param name="channel">The target channel.</param>
    /// <param name="user">The target user.</param>
    /// <param name="timeoutOverride">Overrides the timeout set in <see cref="InteractivityConfiguration.Timeout"/></param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
    public static Task<InteractivityResult<TypingStartEventArgs>> WaitForUserTypingAsync(this DiscordChannel channel, DiscordUser user, TimeSpan? timeoutOverride = null)
        => GetInteractivity(channel).WaitForUserTypingAsync(user, channel, timeoutOverride);

    /// <summary>
    /// Sends a new paginated message.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <param name="user">The user that'll be able to control the pages.</param>
    /// <param name="pages">A collection of <see cref="Page"/> to display.</param>
    /// <param name="emojis">Pagination emojis.</param>
    /// <param name="behaviour">Pagination behaviour (when hitting max and min indices).</param>
    /// <param name="deletion">Deletion behaviour.</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
    public static Task SendPaginatedMessageAsync(this DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationEmojis emojis, PaginationBehaviour? behaviour = default, PaginationDeletion? deletion = default, TimeSpan? timeoutoverride = null)
        => GetInteractivity(channel).SendPaginatedMessageAsync(channel, user, pages, emojis, behaviour, deletion, timeoutoverride);

    /// <summary>
    /// Sends a new paginated message with buttons.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <param name="user">The user that'll be able to control the pages.</param>
    /// <param name="pages">A collection of <see cref="Page"/> to display.</param>
    /// <param name="buttons">Pagination buttons (leave null to default to ones on configuration).</param>
    /// <param name="behaviour">Pagination behaviour.</param>
    /// <param name="deletion">Deletion behaviour</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
    public static Task SendPaginatedMessageAsync(this DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
        => GetInteractivity(channel).SendPaginatedMessageAsync(channel, user, pages, buttons, behaviour, deletion, token);

    /// <inheritdoc cref="SendPaginatedMessageAsync(DiscordChannel, DiscordUser, IEnumerable{Page}, PaginationButtons, PaginationBehaviour?, ButtonPaginationBehavior?, CancellationToken)"/>
    public static Task SendPaginatedMessageAsync(this DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default)
        => channel.SendPaginatedMessageAsync(user, pages, default, behaviour, deletion, token);

    /// <summary>
    /// Sends a new paginated message with buttons.
    /// </summary>
    /// <param name="channel">Target channel.</param>
    /// <param name="user">The user that'll be able to control the pages.</param>
    /// <param name="pages">A collection of <see cref="Page"/> to display.</param>
    /// <param name="buttons">Pagination buttons (leave null to default to ones on configuration).</param>
    /// <param name="behaviour">Pagination behaviour.</param>
    /// <param name="deletion">Deletion behaviour</param>
    /// <param name="timeoutoverride">Override timeout period.</param>
    /// <exception cref="InvalidOperationException">Thrown if interactivity is not enabled for the client associated with the channel.</exception>
    public static Task SendPaginatedMessageAsync(this DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons, TimeSpan? timeoutoverride, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default)
        => GetInteractivity(channel).SendPaginatedMessageAsync(channel, user, pages, buttons, timeoutoverride, behaviour, deletion);

    /// <inheritdoc cref="SendPaginatedMessageAsync(DiscordChannel, DiscordUser, IEnumerable{Page}, PaginationButtons, TimeSpan?, PaginationBehaviour?, ButtonPaginationBehavior?)"/>
    public static Task SendPaginatedMessageAsync(this DiscordChannel channel, DiscordUser user, IEnumerable<Page> pages, TimeSpan? timeoutoverride, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default)
        => channel.SendPaginatedMessageAsync(user, pages, default, timeoutoverride, behaviour, deletion);

    /// <summary>
    /// Retrieves an interactivity instance from a channel instance.
    /// </summary>
    internal static InteractivityExtension GetInteractivity(DiscordChannel channel)
    {
        var client = (DiscordClient)channel.Discord;
        var interactivity = client.GetInteractivity();

        return interactivity ?? throw new InvalidOperationException($"Interactivity is not enabled for this {(client._isShard ? "shard" : "client")}.");
    }
}
