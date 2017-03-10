using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// This is somewhat inspired by Discord.NET 
// asynchronous event code

namespace DSharpPlus
{
    public delegate Task AsyncEventHandler();
    public delegate Task AsyncEventHandler<T>(T e) where T : EventArgs;

    internal sealed class AsyncEvent
    {
        private readonly object _lock = new object();
        private List<AsyncEventHandler> Handlers { get; set; }

        public AsyncEvent()
        {
            this.Handlers = new List<AsyncEventHandler>();
        }

        public void Register(AsyncEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Add(handler);
        }

        public void Unregister(AsyncEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Remove(handler);
        }

        public async Task InvokeAsync()
        {
            if (!this.Handlers.Any())
                return;

            var sc = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            await Task.WhenAll(this.Handlers.Select(xh => xh()));

            SynchronizationContext.SetSynchronizationContext(sc);
        }
    }

    internal sealed class AsyncEvent<T> where T : EventArgs
    {
        private readonly object _lock = new object();
        private List<AsyncEventHandler<T>> Handlers { get; set; }

        public AsyncEvent()
        {
            this.Handlers = new List<AsyncEventHandler<T>>();
        }

        public void Register(AsyncEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Add(handler);
        }

        public void Unregister(AsyncEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Remove(handler);
        }

        public async Task InvokeAsync(T e)
        {
            if (!this.Handlers.Any())
                return;

            var sc = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);

            await Task.WhenAll(this.Handlers.Select(xh => xh(e)));

            SynchronizationContext.SetSynchronizationContext(sc);
        }
    }
}
