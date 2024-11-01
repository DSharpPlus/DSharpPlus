using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

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

    private bool disposed = false;

    public DefaultEventDispatcher
    (
        IServiceProvider serviceProvider,
        IOptions<EventHandlerCollection> handlers,
        IClientErrorHandler errorHandler,
        ILogger<IEventDispatcher> logger
    )
    {
        this.serviceProvider = serviceProvider;
        this.handlers = handlers.Value;
        this.errorHandler = errorHandler;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public ValueTask DispatchAsync<T>(DiscordClient client, T eventArgs)
        where T : DiscordEventArgs
    {
        if (this.disposed)
        {
            return ValueTask.CompletedTask;
        }

        IReadOnlyList<object> general = this.handlers[typeof(DiscordEventArgs)];
        IReadOnlyList<object> specific = this.handlers[typeof(T)];

        IServiceScope scope = this.serviceProvider.CreateScope();

        if (general.Count == 0 && specific.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        _ = Task.WhenAll
        (
            general.Concat(specific)
                .Select(async handler =>
                {
                    try
                    {
                        await ((Func<DiscordClient, T, IServiceProvider, Task>)handler)(client, eventArgs, scope.ServiceProvider);
                    }
                    catch (Exception e)
                    {
                        await this.errorHandler.HandleEventHandlerError(typeof(T).ToString(), e, (Delegate)handler, client, eventArgs);
                    }
                })
        )
        .ContinueWith((_) => scope.Dispose());

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.logger.LogInformation("Detecting shutdown. All further incoming or enqueued events will not dispatch.");
        this.disposed = true;
    }
}
