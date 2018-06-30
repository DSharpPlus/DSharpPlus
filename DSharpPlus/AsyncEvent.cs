﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public delegate Task AsyncEventHandler<T>(T e) where T : AsyncEventArgs;

    /// <summary>
    /// Represents asynchronous event arguments.
    /// </summary>
    public abstract class AsyncEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets or sets whether the event was completely handled. Setting this to true will prevent remaining handlers from running.
        /// </summary>
        public bool Handled { get; set; }
    }

    /// <summary>
    /// Represents an asynchronously-handled event.
    /// </summary>
    public sealed class AsyncEvent
    {
        private readonly object _lock = new object();
        private List<AsyncEventHandler> Handlers { get; }
        private Action<string, Exception> ErrorHandler { get; }
        private string EventName { get; }

        public AsyncEvent(Action<string, Exception> errhandler, string event_name)
        {
            Handlers = new List<AsyncEventHandler>();
            ErrorHandler = errhandler;
            EventName = event_name;
        }

        public void Register(AsyncEventHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            lock (_lock)
            {
                Handlers.Add(handler);
            }
        }

        public void Unregister(AsyncEventHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            lock (_lock)
            {
                Handlers.Remove(handler);
            }
        }

        public async Task InvokeAsync()
        {
            AsyncEventHandler[] handlers = null;
            lock (_lock)
            {
                handlers = Handlers.ToArray();
            }

            if (!handlers.Any())
            {
                return;
            }

            var exs = new List<Exception>(handlers.Length);
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i]().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                }
            }

            if (exs.Any())
            {
                ErrorHandler(EventName, new AggregateException("Exceptions occured within one or more event handlers. Check InnerExceptions for details.", exs));
            }
        }
    }

    /// <summary>
    /// Represents an asynchronously-handled event.
    /// </summary>
    /// <typeparam name="T">Type of EventArgs for this event.</typeparam>
    public sealed class AsyncEvent<T> where T : AsyncEventArgs
    {
        private readonly object _lock = new object();
        private List<AsyncEventHandler<T>> Handlers { get; }
        private Action<string, Exception> ErrorHandler { get; }
        private string EventName { get; }

        public AsyncEvent(Action<string, Exception> errhandler, string event_name)
        {
            Handlers = new List<AsyncEventHandler<T>>();
            ErrorHandler = errhandler;
            EventName = event_name;
        }

        public void Register(AsyncEventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            lock (_lock)
            {
                Handlers.Add(handler);
            }
        }

        public void Unregister(AsyncEventHandler<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");
            }

            lock (_lock)
            {
                Handlers.Remove(handler);
            }
        }

        [DebuggerNonUserCode]
        public async Task InvokeAsync(T e)
        {
            AsyncEventHandler<T>[] handlers = null;
            lock (_lock)
            {
                handlers = Handlers.ToArray();
            }

            if (!handlers.Any())
            {
                return;
            }

            var exs = new List<Exception>(handlers.Length);
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i](e).ConfigureAwait(false);

                    if (e.Handled)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                }
            }

            if (exs.Any())
            {
                ErrorHandler(EventName, new AggregateException("Exceptions occured within one or more event handlers. Check InnerExceptions for details.", exs));
            }
        }
    }
}
