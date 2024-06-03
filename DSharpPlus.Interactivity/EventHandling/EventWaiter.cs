using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "events")]
    private static extern ConcurrentDictionary<Type, AsyncEvent> GetEventsField(DiscordClient client);

    private DiscordClient client;
    private AsyncEvent<DiscordClient, T> @event;
    private AsyncEventHandler<DiscordClient, T> handler;
    private ConcurrentHashSet<MatchRequest<T>> matchrequests;
    private ConcurrentHashSet<CollectRequest<T>> collectrequests;
    private bool disposed = false;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="client">Your DiscordClient</param>
    public EventWaiter(DiscordClient client)
    {
        this.client = client;
        if (GetEventsField(client) is not ConcurrentDictionary<Type, AsyncEvent> events)
        {
            throw new NotImplementedException("Could not get the events from the events field in DiscordClient. This is a bug, please report it.");
        }

        if (!events.TryGetValue(typeof(T), out AsyncEvent? @event))
        {
            throw new InvalidOperationException($"The event {typeof(T).Name} is not registered in the DiscordClient. This is a bug, please report it.");
        }

        this.matchrequests = [];
        this.collectrequests = [];
        this.handler = new AsyncEventHandler<DiscordClient, T>(HandleEvent);
        this.@event = (AsyncEvent<DiscordClient, T>)@event;
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

        if (this.@event != null && this.handler != null)
        {
            this.@event.Unregister(this.handler);
        }

        this.@event = null!;
        this.handler = null!;
        this.client = null!;

        this.matchrequests?.Clear();

        this.collectrequests?.Clear();

        this.matchrequests = null!;
        this.collectrequests = null!;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
