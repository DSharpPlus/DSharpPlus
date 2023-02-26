// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.AsyncEvents
{
    /// <summary>
    /// Provides an implementation of an asynchronous event. Registered handlers are executed asynchronously,
    /// in parallel, and potential exceptions are caught and sent to the specified exception handler.
    /// </summary>
    /// <typeparam name="TSender">The type of the object to dispatch this event.</typeparam>
    /// <typeparam name="TArgs">The type of the argument object for this event.</typeparam>
    public sealed class AsyncEvent<TSender, TArgs> : AsyncEvent
        where TArgs : AsyncEventArgs
    {
        private readonly ReaderWriterLockSlim _lock = new();
        private List<AsyncEventHandler<TSender, TArgs>> _handlers;
        private readonly AsyncEventExceptionHandler<TSender, TArgs> _exceptionHandler;

        public AsyncEvent(string name, AsyncEventExceptionHandler<TSender, TArgs> exceptionHandler)
            : base(name)
        {
            this._handlers = new();
            this._exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Registers a new handler for this event.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if the specified handler was null.</exception>
        public void Register(AsyncEventHandler<TSender, TArgs> handler)
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            this._lock.EnterWriteLock();
            try
            {
                this._handlers.Add(handler);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Unregisters a specific handler from this event.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if the specified handler was null.</exception>
        public void Unregister(AsyncEventHandler<TSender, TArgs> handler)
        {
            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            this._lock.EnterWriteLock();
            try
            {
                this._handlers.Remove(handler);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Unregisters all handlers from this event.
        /// </summary>
        public void UnregisterAll()
            => this._handlers = new();

        /// <summary>
        /// Raises this event, invoking all registered handlers in parallel.
        /// </summary>
        /// <param name="sender">The instance that dispatched this event.</param>
        /// <param name="args">The arguments passed to this event.</param>
        public async Task InvokeAsync(TSender sender, TArgs args)
        {
            if (this._handlers.Count == 0)
                return;

            this._lock.EnterReadLock();

            try
            {
                var tasks = new List<Task>();

                foreach (var handler in this._handlers)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await handler(sender, args);
                        }
                        catch (Exception ex)
                        {
                            this._exceptionHandler(this, ex, handler, sender, args);
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }
            finally
            {
                this._lock.ExitReadLock();
            }

            return;
        }
    }
}
