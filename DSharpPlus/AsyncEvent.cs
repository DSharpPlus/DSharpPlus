﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// This is somewhat inspired by Discord.NET 
// asynchronous event code

namespace DSharpPlus
{
    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <returns>Event handling task.</returns>
    public delegate Task AsyncEventHandler();

    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <typeparam name="T">Type of EventArgs for the event.</typeparam>
    /// <returns>Event handling task.</returns>
    public delegate Task AsyncEventHandler<T>(T e) where T : EventArgs;

    /// <summary>
    /// Represents an asynchronously-handled event.
    /// </summary>
    public sealed class AsyncEvent
    {
        private readonly object _lock = new object();
        internal static readonly object _synclock = new object();
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

            var task = (Task)null;
            lock (_synclock)
            {
                var sc = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);

                task = Task.WhenAll(this.Handlers.Select(xh => xh()));

                SynchronizationContext.SetSynchronizationContext(sc);
            }
            await task;
        }
    }

    /// <summary>
    /// Represents an asynchronously-handled event.
    /// </summary>
    /// <typeparam name="T">Type of EventArgs for this event.</typeparam>
    public sealed class AsyncEvent<T> where T : EventArgs
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

            var task = (Task)null;
            lock (AsyncEvent._synclock)
            {
                var sc = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(null);

                task = Task.WhenAll(this.Handlers.Select(xh => xh(e)));

                SynchronizationContext.SetSynchronizationContext(sc);
            }
            await task;
        }
    }
}
