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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling
{
    /// <summary>
    /// A component-based version of <see cref="EventWaiter{T}"/>
    /// </summary>
    internal class ComponentEventWaiter : IDisposable
    {
        private readonly DiscordClient _client;
        private readonly ConcurrentHashSet<ComponentMatchRequest> _emptyMatchIds = new();
        private readonly ConcurrentDictionary<string, ComponentMatchRequest> _matchRequests = new();
        private readonly ConcurrentDictionary<string, ComponentCollectRequest> _collectRequests = new();

        private readonly DiscordFollowupMessageBuilder _message;
        private readonly InteractivityConfiguration _config;

        public ComponentEventWaiter(DiscordClient client, InteractivityConfiguration config)
        {
            this._client = client;
            this._client.ComponentInteractionCreated += this.Handle;
            this._config = config;

            this._message = new() {Content = config.ResponseMessage ?? "This message was not meant for you.", IsEphemeral = true};
        }

        /// <summary>
        /// Waits for a specified <see cref="ComponentMatchRequest"/>'s predicate to be fufilled.
        /// </summary>
        /// <param name="request">The request to wait for.</param>
        /// <returns>The returned args, or null if it timed out.</returns>
        public async Task<ComponentInteractionCreateEventArgs> WaitForMatchAsync(ComponentMatchRequest request)
        {
            this._matchRequests[request.Id] = request;

            try
            {
                return await request.Tcs.Task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityWaitError, e, "An exception was thrown while waiting for components.");
                return null;
            }
            finally
            {
                this._matchRequests.TryRemove(request.Id, out _);
            }
        }

        /// <summary>
        /// Collects reactions and returns the result when the <see cref="ComponentMatchRequest"/>'s cancellation token is canceled.
        /// </summary>
        /// <param name="request">The request to wait on.</param>
        /// <returns>The result from request's predicate over the period of time leading up to the token's cancellation.</returns>
        public async Task<IReadOnlyList<ComponentInteractionCreateEventArgs>> CollectMatchesAsync(ComponentCollectRequest request)
        {
            this._collectRequests[request.Id] = request;
            try
            {
                await request.Tcs.Task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, e, "There was an error while collecting component event args.");
            }

            return request.Collected.ToArray();
        }

        private async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs args)
        {
            if (this._matchRequests.TryGetValue(args.Id, out var mreq))
            {
                if (mreq.IsMatch(args))
                    mreq.Tcs.TrySetResult(args);

                else if (this._config.ResponseBehavior is InteractionResponseBehavior.Respond)
                    await args.Interaction.CreateFollowupMessageAsync(this._message).ConfigureAwait(false);
            }


            if (this._collectRequests.TryGetValue(args.Id, out var creq))
            {
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);

                if (creq.IsMatch(args))
                    creq.Collected.Add(args);

                else if (this._config.ResponseBehavior is InteractionResponseBehavior.Respond)
                    await args.Interaction.CreateFollowupMessageAsync(this._message).ConfigureAwait(false);
            }
        }
        public void Dispose()
        {
            this._matchRequests.Clear();
            this._collectRequests.Clear();
            this._client.ComponentInteractionCreated -= this.Handle;
        }
    }
}
