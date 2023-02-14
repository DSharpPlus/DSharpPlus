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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling
{
    /// <summary>
    /// Eventwaiter is a class that serves as a layer between the InteractivityExtension
    /// and the DiscordClient to listen to an event and check for matches to a predicate.
    /// </summary>
    internal class ReactionCollector : IDisposable
    {
        DiscordClient _client;

        AsyncEvent<DiscordClient, MessageReactionAddEventArgs> _reactionAddEvent;
        AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs> _reactionAddHandler;

        AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs> _reactionRemoveEvent;
        AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs> _reactionRemoveHandler;

        AsyncEvent<DiscordClient, MessageReactionsClearEventArgs> _reactionClearEvent;
        AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs> _reactionClearHandler;

        ConcurrentHashSet<ReactionCollectRequest> _requests;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="client">Your DiscordClient</param>
        public ReactionCollector(DiscordClient client)
        {
            this._client = client;
            var tinfo = this._client.GetType().GetTypeInfo();

            this._requests = new ConcurrentHashSet<ReactionCollectRequest>();

            // Grabbing all three events from client
            var handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, MessageReactionAddEventArgs>));

            this._reactionAddEvent = (AsyncEvent<DiscordClient, MessageReactionAddEventArgs>)handler.GetValue(this._client);
            this._reactionAddHandler = new AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs>(this.HandleReactionAdd);
            this._reactionAddEvent.Register(this._reactionAddHandler);

            handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>));

            this._reactionRemoveEvent = (AsyncEvent<DiscordClient, MessageReactionRemoveEventArgs>)handler.GetValue(this._client);
            this._reactionRemoveHandler = new AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs>(this.HandleReactionRemove);
            this._reactionRemoveEvent.Register(this._reactionRemoveHandler);

            handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>));

            this._reactionClearEvent = (AsyncEvent<DiscordClient, MessageReactionsClearEventArgs>)handler.GetValue(this._client);
            this._reactionClearHandler = new AsyncEventHandler<DiscordClient, MessageReactionsClearEventArgs>(this.HandleReactionClear);
            this._reactionClearEvent.Register(this._reactionClearHandler);
        }

        public async Task<ReadOnlyCollection<Reaction>> CollectAsync(ReactionCollectRequest request)
        {
            this._requests.Add(request);
            var result = (ReadOnlyCollection<Reaction>)null;

            try
            {
                await request._tcs.Task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, ex, "Exception occurred while collecting reactions");
            }
            finally
            {
                result = new ReadOnlyCollection<Reaction>(new HashSet<Reaction>(request._collected).ToList());
                request.Dispose();
                this._requests.TryRemove(request);
            }
            return result;
        }

        private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
        {
            // foreach request add
            foreach (var req in this._requests)
            {
                if (req._message.Id == eventargs.Message.Id)
                {
                    if (req._collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id != eventargs.User.Id)))
                    {
                        var reaction = req._collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id != eventargs.User.Id));
                        req._collected.TryRemove(reaction);
                        reaction.Users.Add(eventargs.User);
                        req._collected.Add(reaction);
                    }
                    else
                    {
                        req._collected.Add(new Reaction()
                        {
                            Emoji = eventargs.Emoji,
                            Users = new ConcurrentHashSet<DiscordUser>() { eventargs.User }
                        });
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventargs)
        {
            // foreach request remove
            foreach (var req in this._requests)
            {
                if (req._message.Id == eventargs.Message.Id)
                {
                    if (req._collected.Any(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id)))
                    {
                        var reaction = req._collected.First(x => x.Emoji == eventargs.Emoji && x.Users.Any(y => y.Id == eventargs.User.Id));
                        req._collected.TryRemove(reaction);
                        reaction.Users.TryRemove(eventargs.User);
                        if (reaction.Users.Count > 0)
                            req._collected.Add(reaction);
                    }
                }
            }
            return Task.CompletedTask;
        }

        private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
        {
            // foreach request add
            foreach (var req in this._requests)
            {
                if (req._message.Id == eventargs.Message.Id)
                {
                    req._collected.Clear();
                }
            }
            return Task.CompletedTask;
        }

        ~ReactionCollector()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this EventWaiter
        /// </summary>
        public void Dispose()
        {
            this._client = null;

            this._reactionAddEvent.Unregister(this._reactionAddHandler);
            this._reactionRemoveEvent.Unregister(this._reactionRemoveHandler);
            this._reactionClearEvent.Unregister(this._reactionClearHandler);

            this._reactionAddEvent = null;
            this._reactionAddHandler = null;
            this._reactionRemoveEvent = null;
            this._reactionRemoveHandler = null;
            this._reactionClearEvent = null;
            this._reactionClearHandler = null;

            this._requests.Clear();
            this._requests = null;
        }
    }

    public class ReactionCollectRequest : IDisposable
    {
        internal TaskCompletionSource<Reaction> _tcs;
        internal CancellationTokenSource _ct;
        internal TimeSpan _timeout;
        internal DiscordMessage _message;
        internal ConcurrentHashSet<Reaction> _collected;

        public ReactionCollectRequest(DiscordMessage msg, TimeSpan timeout)
        {
            this._message = msg;
            this._collected = new ConcurrentHashSet<Reaction>();
            this._timeout = timeout;
            this._tcs = new TaskCompletionSource<Reaction>();
            this._ct = new CancellationTokenSource(this._timeout);
            this._ct.Token.Register(() => this._tcs.TrySetResult(null));
        }

        ~ReactionCollectRequest()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this._ct.Dispose();
            this._tcs = null;
            this._message = null;
            this._collected?.Clear();
            this._collected = null;
        }
    }

    public class Reaction
    {
        public DiscordEmoji Emoji { get; internal set; }
        public ConcurrentHashSet<DiscordUser> Users { get; internal set; }
        public int Total => this.Users.Count;
    }
}
