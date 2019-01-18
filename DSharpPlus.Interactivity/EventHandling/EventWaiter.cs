using DSharpPlus.Interactivity.Concurrency;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
    public class EventWaiter<T> : IDisposable where T : AsyncEventArgs
    {
        DiscordClient _client;
        AsyncEvent<T> _event;
        AsyncEventHandler<T> _handler;
        ConcurrentHashSet<MatchRequest<T>> _requests;
        object _lock = new object();

        public EventWaiter(DiscordClient client)
        {
            this._client = client;
            var tinfo = _client.GetType().GetTypeInfo();
            var handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<T>));
            this._requests = new ConcurrentHashSet<MatchRequest<T>>();
            this._event = (AsyncEvent<T>)handler.GetValue(_client);
            this._handler = new AsyncEventHandler<T>(HandleEvent);
            this._event.Register(_handler);
        }

        public async Task<T> WaitForMatch(MatchRequest<T> request)
        {
            T result = null;
            this._requests.Add(request);
            try
            {
                result = await request._tcs.Task;
            }
            catch (Exception ex)
            {
                _client.DebugLogger.LogMessage(LogLevel.Error, "Interactivity", $"Something went wrong waiting for {typeof(T).Name}.", DateTime.Now);
            }
            finally
            {
                lock (_lock)
                {
                    this._requests.TryRemove(request);
                }
            }
            return result;
        }

        async Task HandleEvent(T eventargs)
        {
            await Task.Yield();
            foreach(var req in _requests)
            {
                if (req._predicate(eventargs))
                {
                    req._tcs.TrySetResult(eventargs);
                }
            }
        }

        ~EventWaiter()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            this._client = null;
            this._event.Unregister(_handler);
            this._event = null;
            this._handler = null;
            this._lock = null;
        }
    }
}
