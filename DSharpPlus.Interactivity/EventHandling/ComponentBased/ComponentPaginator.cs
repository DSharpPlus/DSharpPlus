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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling
{
    internal class ComponentPaginator : IPaginator
    {
        private readonly DiscordClient _client;
        private readonly Dictionary<ulong, IPaginationRequest> _requests = new();

        public ComponentPaginator(DiscordClient client, InteractivityConfiguration config)
        {
            this._client = client;
            this._client.ComponentInteractionCreated += this.Handle;
        }

        public async Task DoPaginationAsync(IPaginationRequest request)
        {
            var id = (await request.GetMessageAsync().ConfigureAwait(false)).Id;
            this._requests.Add(id, request);

            try
            {
                var tcs = await request.GetTaskCompletionSourceAsync().ConfigureAwait(false);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "There was an exception while paginating.");
            }
            finally
            {
                this._requests.Remove(id);
            }
        }


        private async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs e)
        {
            if (!this._requests.TryGetValue(e.Message.Id, out var req))
                return;

            await this.HandlePaginationAsync(req).ConfigureAwait(false);

        }
        private async Task HandlePaginationAsync(IPaginationRequest request)
        {

        }

        public void Dispose() { }
    }
}
