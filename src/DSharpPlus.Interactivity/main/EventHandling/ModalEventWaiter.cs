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
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// Modal version of <see cref="EventWaiter{T}"/>
/// </summary>
internal class ModalEventWaiter : IDisposable
{
    private DiscordClient Client { get; }

    /// <summary>
    /// Collection of <see cref = "ModalMatchRequest"/> representing requests to wait for modals.
    /// </summary>
    private ConcurrentHashSet<ModalMatchRequest> MatchRequests { get; } = new();

    public ModalEventWaiter(DiscordClient client)
    {
        this.Client = client;
        this.Client.ModalSubmitted += this.Handle; //registering Handle event to be fired upon ModalSubmitted
    }

    /// <summary>
    /// Waits for a specified <see cref="ModalMatchRequest"/>'s predicate to be fulfilled.
    /// </summary>
    /// <param name="request">The request to wait for a match.</param>
    /// <returns>The returned args, or null if it timed out.</returns>
    public async Task<ModalSubmitEventArgs> WaitForMatchAsync(ModalMatchRequest request)
    {
        this.MatchRequests.Add(request);

        try
        {
            return await request.Tcs.Task.ConfigureAwait(false); // awaits request until completion or cancellation
        }
        catch (Exception e)
        {
            this.Client.Logger.LogError(InteractivityEvents.InteractivityWaitError, e, "An exception was thrown while waiting for a modal.");
            return null;
        }
        finally
        {
            this.MatchRequests.TryRemove(request);
        }
    }

    /// <summary>
    /// Is called whenever <see cref="ModalSubmitEventArgs"/> is fired. Checks to see submitted modal matches any of the current requests.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args">The <see cref="ModalSubmitEventArgs"/> to match.</param>
    /// <returns>A task that represents matching the requests.</returns>
    private Task Handle(DiscordClient _, ModalSubmitEventArgs args)
    {
        foreach (var req in this.MatchRequests.ToArray()) // ToArray to get a copy of the collection that won't be modified during iteration
        {
            if (req.ModalId == args.Interaction.Data.CustomId && req.IsMatch(args)) // will catch all matches
                req.Tcs.TrySetResult(args);
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        this.MatchRequests.Clear();
        this.Client.ModalSubmitted -= this.Handle;
    }
}
