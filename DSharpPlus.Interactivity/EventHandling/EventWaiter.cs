using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.AsyncEvents;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// Eventwaiter is a class that serves as a layer between the InteractivityExtension
/// and the DiscordClient to listen to an event and check for matches to a predicate.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class EventWaiter<T> : IDisposable where T : AsyncEventArgs
{
    private DiscordClient client;
    private AsyncEvent<DiscordClient, T> @event;
    private AsyncEventHandler<DiscordClient, T> handler;
    private ConcurrentHashSet<MatchRequest<T>> matchrequests;
    private ConcurrentHashSet<CollectRequest<T>> collectrequests;
    private bool disposed = false;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="extension">The extension to register to.</param>
    public EventWaiter(InteractivityExtension extension)
    {
        this.client = extension.Client;

        this.@event = (AsyncEvent<DiscordClient, T>)extension.eventDistributor.GetOrAdd
        (
            typeof(T),
            new AsyncEvent<DiscordClient, T>(extension.errorHandler)
        );

        this.matchrequests = [];
        this.collectrequests = [];
        this.handler = new AsyncEventHandler<DiscordClient, T>(HandleEvent);
        this.@event.Register(this.handler);
    }

    /// <summary>
    /// Waits for a match to a specific request, else returns null.
    /// </summary>
    /// <param name="request">Request to match</param>
    /// <returns></returns>
    public async Task<T> WaitForMatchAsync(MatchRequest<T> request)
    {
        T result = null;
        this.matchrequests.Add(request);
        try
        {
            result = await request.tcs.Task;
        }
        catch (Exception ex)
        {
            this.client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while waiting for {Request}", typeof(T).Name);
        }
        finally
        {
            request.Dispose();
            this.matchrequests.TryRemove(request);
        }

        return result;
    }

    public async Task<ReadOnlyCollection<T>> CollectMatchesAsync(CollectRequest<T> request)
    {
        ReadOnlyCollection<T> result;
        this.collectrequests.Add(request);
        try
        {
            await request.tcs.Task;
        }
        catch (Exception ex)
        {
            this.client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while collecting from {Request}", typeof(T).Name);
        }
        finally
        {
            result = new ReadOnlyCollection<T>(new HashSet<T>(request.collected).ToList());
            request.Dispose();
            this.collectrequests.TryRemove(request);
        }
        return result;
    }

    private Task HandleEvent(DiscordClient client, T eventargs)
    {
        if (!this.disposed)
        {
            foreach (MatchRequest<T> req in this.matchrequests)
            {
                if (req.predicate(eventargs))
                {
                    req.tcs.TrySetResult(eventargs);
                }
            }

            foreach (CollectRequest<T> req in this.collectrequests)
            {
                if (req.predicate(eventargs))
                {
                    req.collected.Add(eventargs);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes this EventWaiter
    /// </summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }
        
        this.disposed = true;
        this.matchrequests.Clear();
        this.collectrequests.Clear();

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
