using DSharpPlus.Interactivity.Concurrency;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        AsyncEvent<T> _event;
        AsyncEventHandler<T> _handler;
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
            var handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<T>));
            this._matchrequests = new ConcurrentHashSet<MatchRequest<T>>();
            this._collectrequests = new ConcurrentHashSet<CollectRequest<T>>();
            this._event = (AsyncEvent<T>)handler.GetValue(_client);
            this._handler = new AsyncEventHandler<T>(HandleEvent);
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
                this._client.DebugLogger.LogMessage(LogLevel.Error, "Interactivity", 
                    $"Something went wrong waiting for {typeof(T).Name} with exception {ex.GetType().Name}.", DateTime.Now);
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
                this._client.DebugLogger.LogMessage(LogLevel.Error, "Interactivity", 
                    $"Something went wrong collecting from {typeof(T).Name} with exception {ex.GetType().Name}.", DateTime.Now);
            }
            finally
            {
                result = new ReadOnlyCollection<T>(new HashSet<T>(request._collected).ToList());
                request.Dispose();
                this._collectrequests.TryRemove(request);
            }
            return result;
        }

        async Task HandleEvent(T eventargs)
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
