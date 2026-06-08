using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Clients;

/// <summary>
/// The default DSharpPlus event dispatcher, dispatching events asynchronously and using a shared scope. Catch-all event
/// handlers referencing <see cref="DiscordEventArgs"/> are supported.
/// </summary>
public sealed class DefaultEventDispatcher : IEventDispatcher, IDisposable
{
    private readonly IServiceProvider serviceProvider;
    private readonly EventHandlerCollection handlers;
    private readonly IClientErrorHandler errorHandler;
    private readonly ILogger<IEventDispatcher> logger;
    private readonly IMemoryCache cache;
    private readonly ConcurrentDictionary<Ulid, InFlightEventWaiter> inFlightWaiters;
    private readonly ConcurrentDictionary<Ulid, InFlightEventCollector> inFlightCollectors;


    private bool disposed = false;

    public DefaultEventDispatcher
    (
        IServiceProvider serviceProvider,
        IOptions<EventHandlerCollection> handlers,
        IClientErrorHandler errorHandler,
        ILogger<IEventDispatcher> logger,
        IMemoryCache cache
    )
    {
        this.serviceProvider = serviceProvider;
        this.handlers = handlers.Value;
        this.errorHandler = errorHandler;
        this.logger = logger;
        this.cache = cache;
        this.inFlightWaiters = [];
        this.inFlightCollectors = [];
    }

    /// <inheritdoc/>
    public EventWaiter<T> CreateEventWaiter<T>(Func<T, bool> condition, TimeSpan timeout) 
        where T : DiscordEventArgs
    {
        EventWaiter<T> waiter = new()
        {
            Id = Ulid.NewUlid(),
            Condition = condition,
            CompletionSource = new(),
            Timeout = timeout
        };

        InFlightEventWaiter inFlight = new(waiter.Id, Unsafe.As<Func<DiscordEventArgs, bool>>(condition), typeof(T));
        
        this.cache.CreateEntry(waiter.Id)
            .SetAbsoluteExpiration(timeout)
            .SetValue(waiter)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                if (reason == EvictionReason.Expired)
                {
                    this.inFlightWaiters.Remove((Ulid)key, out _);
                    ((EventWaiter<T>)value!).CompletionSource.SetResult(EventWaiterResult<T>.FromTimedOut());
                }
            })
            .Dispose();

        this.inFlightWaiters.AddOrUpdate(waiter.Id, inFlight, (_, _) => inFlight);

        return waiter;
    }

    /// <inheritdoc/>
    public EventCollector<T> CreateEventCollector<T>(Func<T, bool> condition, TimeSpan timeout)
        where T : DiscordEventArgs
    {
        EventCollector<T> collector = new()
        {
            Id = Ulid.NewUlid(),
            Condition = condition,
            Timeout = timeout
        };

        InFlightEventCollector inFlight = new(collector.Id, Unsafe.As<Func<DiscordEventArgs, bool>>(condition), typeof(T));

        this.cache.CreateEntry(collector.Id)
            .SetAbsoluteExpiration(timeout)
            .SetValue(collector)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                if (reason == EvictionReason.Expired)
                {
                    this.inFlightCollectors.Remove((Ulid)key, out _);
                    EventCollector<T> retrieved = (EventCollector<T>)value!;
                    retrieved.CompletionSource.SetResult(retrieved.CollectedEvents.ToArray());
                }
            })
            .Dispose();

        this.inFlightCollectors.AddOrUpdate(collector.Id, inFlight, (_, _) => inFlight);

        return collector;
    }

    /// <inheritdoc/>
    public ValueTask DispatchAsync<T>(DiscordClient client, T eventArgs)
        where T : DiscordEventArgs
    {
        if (this.disposed)
        {
            return ValueTask.CompletedTask;
        }

        _ = Parallel.ForEachAsync(this.inFlightWaiters.Values, (waiter, ct) =>
        {
            if (waiter.EventType != typeof(T))
            {
                return ValueTask.CompletedTask;
            }

            if (!waiter.Condition(eventArgs))
            {
                return ValueTask.CompletedTask;
            }

            EventWaiter<T>? fullWaiter = this.cache.Get<EventWaiter<T>>(waiter.Id);

            fullWaiter?.CompletionSource.SetResult(new()
            {
                TimedOut = false,
                Value = eventArgs
            });

            this.inFlightWaiters.Remove(waiter.Id, out _);
            this.cache.Remove(waiter.Id);

            return ValueTask.CompletedTask;
        });

        _ = Parallel.ForEachAsync(this.inFlightCollectors.Values, (collector, ct) =>
        {
            if (collector.EventType != typeof(T))
            {
                return ValueTask.CompletedTask;
            }

            if (!collector.Condition(eventArgs))
            {
                return ValueTask.CompletedTask;
            }

            EventCollector<T>? fullCollector = this.cache.Get<EventCollector<T>>(collector.Id);

            fullCollector?.CollectedEvents.Add(eventArgs);

            return ValueTask.CompletedTask;
        });

        IReadOnlyList<object> general = this.handlers[typeof(DiscordEventArgs)];
        IReadOnlyList<object> specific = this.handlers[typeof(T)];

        if (general.Count == 0 && specific.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        try
        {
            IServiceScope scope = this.serviceProvider.CreateScope();
            _ = Task.WhenAll
                (
                    general.Concat(specific)
                        .Select(async handler =>
                        {
                            try
                            {
                                await ((Func<DiscordClient, T, IServiceProvider, Task>)handler)(client, eventArgs,
                                    scope.ServiceProvider);
                            }
                            catch (Exception e)
                            {
                                await this.errorHandler.HandleEventHandlerError(typeof(T).ToString(), e,
                                    (Delegate)handler, client, eventArgs);
                            }
                        })
                )
                .ContinueWith((_) => scope.Dispose());
        }
        catch (ObjectDisposedException)
        {
            // ObjectDisposedException can be thrown from the this.serviceProvider.CreateScope() call above,
            // when the serviceProvider is already disposed externally.
            // This *should* only happen when the hosting application is shutting down,
            // so it should be safe to just ignore it.
            // One option would be to show that exception as debug log, but I would guess that just causes confusion.
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.logger.LogInformation("Detecting shutdown. All further incoming or enqueued events will not dispatch.");
        this.disposed = true;
    }

    private readonly record struct InFlightEventWaiter(Ulid Id, Func<DiscordEventArgs, bool> Condition, Type EventType);

    private readonly record struct InFlightEventCollector(Ulid Id, Func<DiscordEventArgs, bool> Condition, Type EventType);
}
