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
        List<MatchRequest<T>> _requests;
        object _lock = new object();

        public EventWaiter(DiscordClient client)
        {
            this._client = client;
            var tinfo = _client.GetType().GetTypeInfo();
            var handler = tinfo.DeclaredFields.First(x => x.FieldType == typeof(AsyncEvent<T>));
            this._requests = new List<MatchRequest<T>>();
            this._event = (AsyncEvent<T>)handler.GetValue(_client);
            this._handler = new AsyncEventHandler<T>(HandleEvent);
            this._event.Register(_handler);
        }

        public async Task<T> WaitForMatch(MatchRequest<T> request)
        {
            T result = null;
            lock (_lock)
            {
                this._requests.Add(request);
            }
            try
            {
                result = await request._tcs.Task;
            }
            catch(Exception ex)
            {
                _client.DebugLogger.LogMessage(LogLevel.Error, "Interactivity", $"Something went wrong waiting for {typeof(T).Name}.", DateTime.Now);
            }
            finally
            {
                lock (_lock)
                {
                    this._requests.Remove(request);
                }
            }
            return result;
        }

        async Task HandleEvent(T eventargs)
        {
            await Task.Yield();
            lock (_lock)
            {
                for (int i = 0; i < _requests.Count; i++)
                {
                    var req = _requests[i];
                    if (req._predicate(eventargs))
                    {
                        req._tcs.SetResult(eventargs);
                    }
                }
            }
        }

        ~EventWaiter()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            // TODO: do disposal, unset vars etc
            _client = null;
            _event.Unregister(_handler);
            _event = null;
        }
    }
}
