using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
    private DiscordClient _client;
    private AsyncEvent<DiscordClient, T> _event;
    private AsyncEventHandler<DiscordClient, T> _handler;
    private ConcurrentHashSet<MatchRequest<T>> _matchrequests;
    private ConcurrentHashSet<CollectRequest<T>> _collectrequests;
    private bool _disposed = false;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="client">Your DiscordClient</param>
    public EventWaiter(DiscordClient client)
    {
        _client = client;
        TypeInfo tinfo = _client.GetType().GetTypeInfo();
        FieldInfo handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, T>));
        _matchrequests = new ConcurrentHashSet<MatchRequest<T>>();
        _collectrequests = new ConcurrentHashSet<CollectRequest<T>>();
        _event = (AsyncEvent<DiscordClient, T>)handler.GetValue(_client);
        _handler = new AsyncEventHandler<DiscordClient, T>(HandleEvent);
        _event.Register(_handler);
    }

    /// <summary>
    /// Waits for a match to a specific request, else returns null.
    /// </summary>
    /// <param name="request">Request to match</param>
    /// <returns></returns>
    public async Task<T> WaitForMatch(MatchRequest<T> request)
    {
        T result = null;
        _matchrequests.Add(request);
        try
        {
            result = await request._tcs.Task;
        }
        catch (Exception ex)
        {
            _client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while waiting for {Request}", typeof(T).Name);
        }
        finally
        {
            request.Dispose();
            _matchrequests.TryRemove(request);
        }
        return result;
    }

    public async Task<ReadOnlyCollection<T>> CollectMatches(CollectRequest<T> request)
    {
        ReadOnlyCollection<T> result = null;
        _collectrequests.Add(request);
        try
        {
            await request._tcs.Task;
        }
        catch (Exception ex)
        {
            _client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while collecting from {Request}", typeof(T).Name);
        }
        finally
        {
            result = new ReadOnlyCollection<T>(new HashSet<T>(request._collected).ToList());
            request.Dispose();
            _collectrequests.TryRemove(request);
        }
        return result;
    }

    private Task HandleEvent(DiscordClient client, T eventargs)
    {
        if (!_disposed)
        {
            foreach (MatchRequest<T> req in _matchrequests)
            {
                if (req._predicate(eventargs))
                {
                    req._tcs.TrySetResult(eventargs);
                }
            }

            foreach (CollectRequest<T> req in _collectrequests)
            {
                if (req._predicate(eventargs))
                {
                    req._collected.Add(eventargs);
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
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_event != null && _handler != null)
        {
            _event.Unregister(_handler);
        }

        _event = null!;
        _handler = null!;
        _client = null!;

        _matchrequests?.Clear();

        _collectrequests?.Clear();

        _matchrequests = null!;
        _collectrequests = null!;

        // Satisfy rule CA1816. Can be removed if this class is sealed.
        GC.SuppressFinalize(this);
    }
}
