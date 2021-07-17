// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling
{
    internal class Paginator : IPaginator
    {
        DiscordClient _client;
        ConcurrentHashSet<IPaginationRequest> _requests;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="client">Your DiscordClient</param>
        public Paginator(DiscordClient client)
        {
            this._client = client;
            this._requests = new ConcurrentHashSet<IPaginationRequest>();

            this._client.MessageReactionAdded += this.HandleReactionAdd;
            this._client.MessageReactionRemoved += this.HandleReactionRemove;
            this._client.MessageReactionsCleared += this.HandleReactionClear;
        }

        public async Task DoPaginationAsync(IPaginationRequest request)
        {
            await this.ResetReactionsAsync(request).ConfigureAwait(false);
            this._requests.Add(request);
            try
            {
                var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
                await tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
            }
            finally
            {
                this._requests.TryRemove(request);
                try
                {
                    await request.DoCleanupAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
                }
            }
        }

        private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                foreach (var req in this._requests)
                {
                    var emojis = await req.GetEmojisAsync().ConfigureAwait(false);
                    var msg = await req.GetMessageAsync().ConfigureAwait(false);
                    var usr = await req.GetUserAsync().ConfigureAwait(false);

                    if (msg.Id == eventargs.Message.Id)
                    {
                        if (eventargs.User.Id == usr.Id)
                        {
                            if (req.PageCount > 1 &&
                                (eventargs.Emoji == emojis.Left ||
                                 eventargs.Emoji == emojis.SkipLeft ||
                                 eventargs.Emoji == emojis.Right ||
                                 eventargs.Emoji == emojis.SkipRight ||
                                 eventargs.Emoji == emojis.Stop))
                            {
                                await this.PaginateAsync(req, eventargs.Emoji).ConfigureAwait(false);
                            }
                            else if (eventargs.Emoji == emojis.Stop &&
                                     req is PaginationRequest paginationRequest &&
                                     paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                            {
                                await this.PaginateAsync(req, eventargs.Emoji).ConfigureAwait(false);
                            }
                            else
                            {
                                await msg.DeleteReactionAsync(eventargs.Emoji, eventargs.User).ConfigureAwait(false);
                            }
                        }
                        else if (eventargs.User.Id != this._client.CurrentUser.Id)
                        {
                            if (eventargs.Emoji != emojis.Left &&
                               eventargs.Emoji != emojis.SkipLeft &&
                               eventargs.Emoji != emojis.Right &&
                               eventargs.Emoji != emojis.SkipRight &&
                               eventargs.Emoji != emojis.Stop)
                            {
                                await msg.DeleteReactionAsync(eventargs.Emoji, eventargs.User).ConfigureAwait(false);
                            }
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventargs)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                foreach (var req in this._requests)
                {
                    var emojis = await req.GetEmojisAsync().ConfigureAwait(false);
                    var msg = await req.GetMessageAsync().ConfigureAwait(false);
                    var usr = await req.GetUserAsync().ConfigureAwait(false);

                    if (msg.Id == eventargs.Message.Id)
                    {
                        if (eventargs.User.Id == usr.Id)
                        {
                            if (req.PageCount > 1 &&
                                (eventargs.Emoji == emojis.Left ||
                                 eventargs.Emoji == emojis.SkipLeft ||
                                 eventargs.Emoji == emojis.Right ||
                                 eventargs.Emoji == emojis.SkipRight ||
                                 eventargs.Emoji == emojis.Stop))
                            {
                                await this.PaginateAsync(req, eventargs.Emoji).ConfigureAwait(false);
                            }
                            else if (eventargs.Emoji == emojis.Stop &&
                                     req is PaginationRequest paginationRequest &&
                                     paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                            {
                                await this.PaginateAsync(req, eventargs.Emoji).ConfigureAwait(false);
                            }
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                foreach (var req in this._requests)
                {
                    var msg = await req.GetMessageAsync().ConfigureAwait(false);

                    if (msg.Id == eventargs.Message.Id)
                    {
                        await this.ResetReactionsAsync(req).ConfigureAwait(false);
                    }
                }
            });

            return Task.CompletedTask;
        }

        private async Task ResetReactionsAsync(IPaginationRequest p)
        {
            var msg = await p.GetMessageAsync().ConfigureAwait(false);
            var emojis = await p.GetEmojisAsync().ConfigureAwait(false);

            // Test permissions to avoid a 403:
            // https://totally-not.a-sketchy.site/3pXpRLK.png
            // Yes, this is an issue
            // No, we should not require people to guarantee MANAGE_MESSAGES
            // Need to check following:
            // - In guild?
            //  - If yes, check if have permission
            // - If all above fail (DM || guild && no permission), skip this
            var chn = msg.Channel;
            var gld = chn?.Guild;
            var mbr = gld?.CurrentMember;

            if (mbr != null /* == is guild and cache is valid */ && (chn.PermissionsFor(mbr) & Permissions.ManageChannels) != 0) /* == has permissions */
                await msg.DeleteAllReactionsAsync("Pagination").ConfigureAwait(false);
            // ENDOF: 403 fix

            if (p.PageCount > 1)
            {
                if (emojis.SkipLeft != null)
                    await msg.CreateReactionAsync(emojis.SkipLeft).ConfigureAwait(false);
                if (emojis.Left != null)
                    await msg.CreateReactionAsync(emojis.Left).ConfigureAwait(false);
                if (emojis.Right != null)
                    await msg.CreateReactionAsync(emojis.Right).ConfigureAwait(false);
                if (emojis.SkipRight != null)
                    await msg.CreateReactionAsync(emojis.SkipRight).ConfigureAwait(false);
                if (emojis.Stop != null)
                    await msg.CreateReactionAsync(emojis.Stop).ConfigureAwait(false);
            }
            else if (emojis.Stop != null && p is PaginationRequest paginationRequest && paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
            {
                await msg.CreateReactionAsync(emojis.Stop).ConfigureAwait(false);
            }
        }

        private async Task PaginateAsync(IPaginationRequest p, DiscordEmoji emoji)
        {
            var emojis = await p.GetEmojisAsync().ConfigureAwait(false);
            var msg = await p.GetMessageAsync().ConfigureAwait(false);

            if (emoji == emojis.SkipLeft)
                await p.SkipLeftAsync().ConfigureAwait(false);
            else if (emoji == emojis.Left)
                await p.PreviousPageAsync().ConfigureAwait(false);
            else if (emoji == emojis.Right)
                await p.NextPageAsync().ConfigureAwait(false);
            else if (emoji == emojis.SkipRight)
                await p.SkipRightAsync().ConfigureAwait(false);
            else if (emoji == emojis.Stop)
            {
                var tcs = await p.GetTaskCompletionSourceAsync().ConfigureAwait(false);
                tcs.TrySetResult(true);
                return;
            }

            var page = await p.GetPageAsync().ConfigureAwait(false);
            var builder = new DiscordMessageBuilder()
                .WithContent(page.Content)
                .WithEmbed(page.Embed);

            await builder.ModifyAsync(msg).ConfigureAwait(false);
        }

        ~Paginator()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this EventWaiter
        /// </summary>
        public void Dispose()
        {
            this._client.MessageReactionAdded -= this.HandleReactionAdd;
            this._client.MessageReactionRemoved -= this.HandleReactionRemove;
            this._client.MessageReactionsCleared -= this.HandleReactionClear;
            this._client = null;
            this._requests.Clear();
            this._requests = null;
        }
    }
}
