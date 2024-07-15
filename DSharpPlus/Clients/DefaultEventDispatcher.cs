using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Handler = System.Func<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.DiscordEventArgs, System.IServiceProvider, System.Threading.Tasks.Task>;

namespace DSharpPlus.Clients;

/// <summary>
/// The default DSharpPlus event dispatcher, dispatching events asynchronously and using a shared scope. Catch-all event
/// handlers referencing <see cref="DiscordEventArgs"/> are supported.
/// </summary>
public sealed class DefaultEventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider serviceProvider;
    private readonly EventHandlerCollection handlers;
    private readonly IClientErrorHandler errorHandler;
    private readonly ILogger<IEventDispatcher> logger;

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
        IReadOnlyList<Handler> general = this.handlers[typeof(DiscordEventArgs)];
        IReadOnlyList<Handler> specific = this.handlers[typeof(T)];

        using IServiceScope scope = this.serviceProvider.CreateScope();

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
                        await handler(client, eventArgs, scope.ServiceProvider);
                    }
                    catch (Exception e)
                    {
                        await this.errorHandler.HandleEventHandlerError(typeof(T).ToString(), e, handler, client, eventArgs);
                    }
                })
        );

        return ValueTask.CompletedTask;
    }
}
