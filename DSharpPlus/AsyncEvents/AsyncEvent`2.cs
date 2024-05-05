using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.AsyncEvents;

/// <summary>
/// Provides an implementation of an asynchronous event. Registered handlers are executed asynchronously,
/// in parallel, and potential exceptions are caught and sent to the specified exception handler.
/// </summary>
/// <typeparam name="TSender">The type of the object to dispatch this event.</typeparam>
/// <typeparam name="TArgs">The type of the argument object for this event.</typeparam>
public sealed class AsyncEvent<TSender, TArgs> : AsyncEvent
    where TArgs : AsyncEventArgs
{
    private readonly SemaphoreSlim @lock = new(1);
    private readonly AsyncEventExceptionHandler<TSender, TArgs> exceptionHandler;
    private List<AsyncEventHandler<TSender, TArgs>> handlers;

    public AsyncEvent(string name, AsyncEventExceptionHandler<TSender, TArgs> exceptionHandler)
        : base(name)
    {
        this.handlers = [];
        this.exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Registers a new handler for this event.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the specified handler was null.</exception>
    public void Register(AsyncEventHandler<TSender, TArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        this.@lock.Wait();
        try
        {
            this.handlers.Add(handler);
        }
        finally
        {
            this.@lock.Release();
        }
    }

    /// <summary>
    /// Unregisters a specific handler from this event.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the specified handler was null.</exception>
    public void Unregister(AsyncEventHandler<TSender, TArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        this.@lock.Wait();
        try
        {
            this.handlers.Remove(handler);
        }
        finally
        {
            this.@lock.Release();
        }
    }

    /// <summary>
    /// Unregisters all handlers from this event.
    /// </summary>
    public void UnregisterAll()
        => this.handlers = [];

    /// <summary>
    /// Raises this event, invoking all registered handlers in parallel.
    /// </summary>
    /// <param name="sender">The instance that dispatched this event.</param>
    /// <param name="args">The arguments passed to this event.</param>
    public async Task InvokeAsync(TSender sender, TArgs args)
    {
        if (this.handlers.Count == 0)
        {
            return;
        }

        await this.@lock.WaitAsync();
        List<AsyncEventHandler<TSender, TArgs>> copiedHandlers = new(this.handlers);
        this.@lock.Release();

        _ = Task.WhenAll(copiedHandlers.Select(async (handler) =>
        {
            try
            {
                await handler(sender, args);
            }
            catch (Exception ex)
            {
                this.exceptionHandler?.Invoke(this, ex, handler, sender, args);
            }
        }));

        return;
    }
}
