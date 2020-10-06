﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConcurrentCollections;
using Emzi0767.Utilities;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling
{
    /// <summary>
    /// Eventwaiter is a class that serves as a layer between the InteractivityExtension
    /// and the DiscordClient to listen to an event and check for matches to a predicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class EventWaiter<T> : IDisposable where T : AsyncEventArgs
    {
        DiscordClient _client;
        AsyncEvent<DiscordClient, T> _event;
        AsyncEventHandler<DiscordClient, T> _handler;
        ConcurrentHashSet<MatchRequest<T>> _matchrequests;
        ConcurrentHashSet<CollectRequest<T>> _collectrequests;
        bool disposed = false;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="client">Your DiscordClient</param>
        public EventWaiter(DiscordClient client)
        {
            this._client = client;
            var tinfo = _client.GetType().GetTypeInfo();
            var handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<DiscordClient, T>));
            this._matchrequests = new ConcurrentHashSet<MatchRequest<T>>();
            this._collectrequests = new ConcurrentHashSet<CollectRequest<T>>();
            this._event = (AsyncEvent<DiscordClient, T>)handler.GetValue(_client);
            this._handler = new AsyncEventHandler<DiscordClient, T>(HandleEvent);
            this._event.Register(_handler);
        }

        /// <summary>
        /// Waits for a match to a specific request, else returns null.
        /// </summary>
        /// <param name="request">Request to match</param>
        /// <returns></returns>
        public async Task<T> WaitForMatch(MatchRequest<T> request)
        {
            T result = null;
            this._matchrequests.Add(request);
            try
            {
                result = await request._tcs.Task;
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while waiting for {0}", typeof(T).Name);
            }
            finally
            {
                request.Dispose();
                this._matchrequests.TryRemove(request);
            }
            return result;
        }

        public async Task<ReadOnlyCollection<T>> CollectMatches(CollectRequest<T> request)
        {
            ReadOnlyCollection<T> result = null;
            this._collectrequests.Add(request);
            try
            {
                await request._tcs.Task;
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityWaitError, ex, "An exception occurred while collecting from {0}", typeof(T).Name);
            }
            finally
            {
                result = new ReadOnlyCollection<T>(new HashSet<T>(request._collected).ToList());
                request.Dispose();
                this._collectrequests.TryRemove(request);
            }
            return result;
        }

        async Task HandleEvent(DiscordClient client, T eventargs)
        {
            await Task.Yield();
            if (!disposed)
            {
                foreach (var req in _matchrequests)
                {
                    if (req._predicate(eventargs))
                    {
                        req._tcs.TrySetResult(eventargs);
                    }
                }
            }

            if (!disposed)
            {
                foreach (var req in _collectrequests)
                {
                    if (req._predicate(eventargs))
                    {
                        req._collected.Add(eventargs);
                    }
                }
            }
        }

        ~EventWaiter()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this EventWaiter
        /// </summary>
        public void Dispose()
        {
            this.disposed = true;
            if(this._event != null)
                this._event.Unregister(_handler);

            this._event = null;
            this._handler = null;
            this._client = null;

            if(this._matchrequests != null)
                this._matchrequests.Clear();
            if(this._collectrequests != null)
                this._collectrequests.Clear();

            this._matchrequests = null;
            this._collectrequests = null;
        }
    }
}
