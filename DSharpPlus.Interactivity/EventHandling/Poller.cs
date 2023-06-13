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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling
{
    internal class Poller
    {
        DiscordClient _client;
        ConcurrentHashSet<PollRequest> _requests;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="client">Your DiscordClient</param>
        public Poller(DiscordClient client)
        {
            this._client = client;
            this._requests = new ConcurrentHashSet<PollRequest>();

            this._client.MessageReactionAdded += this.HandleReactionAdd;
            this._client.MessageReactionRemoved += this.HandleReactionRemove;
            this._client.MessageReactionsCleared += this.HandleReactionClear;
        }

        public async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(PollRequest request)
        {
            ReadOnlyCollection<PollEmoji> result = null;
            this._requests.Add(request);
            try
            {
                await request._tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPollError, ex, "Exception occurred while polling");
            }
            finally
            {
                result = new ReadOnlyCollection<PollEmoji>(new HashSet<PollEmoji>(request._collected).ToList());
                request.Dispose();
                this._requests.TryRemove(request);
            }
            return result;
        }

        private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
        {
            if (this._requests.Count == 0)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                foreach (var req in this._requests)
                {
                    // match message
                    if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
                    {
                        if (req._emojis.Contains(eventargs.Emoji) && !req._collected.Any(x => x.Voted.Contains(eventargs.User)))
                        {
                            if (eventargs.User.Id != this._client.CurrentUser.Id)
                                req.AddReaction(eventargs.Emoji, eventargs.User);
                        }
                        else
                        {
                            var member = await eventargs.Channel.Guild.GetMemberAsync(client.CurrentUser.Id).ConfigureAwait(false);
                            if (eventargs.Channel.PermissionsFor(member).HasPermission(Permissions.ManageMessages))
                                await eventargs.Message.DeleteReactionAsync(eventargs.Emoji, eventargs.User).ConfigureAwait(false);
                        }
                    }
                }
            });
            return Task.CompletedTask;
        }

        private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventargs)
        {
            foreach (var req in this._requests)
            {
                // match message
                if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
                {
                    if (eventargs.User.Id != this._client.CurrentUser.Id)
                        req.RemoveReaction(eventargs.Emoji, eventargs.User);
                }
            }
            return Task.CompletedTask;
        }

        private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
        {
            foreach (var req in this._requests)
            {
                // match message
                if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
                {
                    req.ClearCollected();
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes this EventWaiter
        /// </summary>
        public void Dispose()
        {
            // Why doesn't this class implement IDisposable?

            if (this._client != null)
            {
                this._client.MessageReactionAdded -= this.HandleReactionAdd;
                this._client.MessageReactionRemoved -= this.HandleReactionRemove;
                this._client.MessageReactionsCleared -= this.HandleReactionClear;
                this._client = null!;
            }

            if (this._requests != null)
            {
                this._requests.Clear();
                this._requests = null!;
            }
        }
    }
}
