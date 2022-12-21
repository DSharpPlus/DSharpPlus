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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

namespace DSharpPlus.Interactivity.Extensions;

public static class InteractionExtensions
{
    /// <summary>
    /// Sends a paginated message in response to an interaction.
    /// <para>
    /// <b>Pass the interaction directly. Interactivity will ACK it.</b>
    /// </para>
    /// </summary>
    /// <param name="interaction">The interaction to create a response to.</param>
    /// <param name="ephemeral">Whether the response should be ephemeral.</param>
    /// <param name="user">The user to listen for button presses from.</param>
    /// <param name="pages">The pages to paginate.</param>
    /// <param name="buttons">Optional: custom buttons</param>
    /// <param name="behaviour">Pagination behaviour.</param>
    /// <param name="deletion">Deletion behaviour</param>
    /// <param name="token">A custom cancellation token that can be cancelled at any point.</param>
    /// <param name="asEditResponse">If the response as edit of previous response.</param>
    public static Task SendPaginatedResponseAsync(this DiscordInteraction interaction, bool ephemeral, DiscordUser user, IEnumerable<Page> pages, PaginationButtons buttons = null, PaginationBehaviour? behaviour = default, ButtonPaginationBehavior? deletion = default, CancellationToken token = default, bool asEditResponse = false)
        => ChannelExtensions.GetInteractivity(interaction.Channel).SendPaginatedResponseAsync(interaction, ephemeral, user, pages, buttons, behaviour, deletion, token, asEditResponse);
}
