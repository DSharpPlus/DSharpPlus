using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DSharpPlus.Interactivity.Components;

/// <summary>
/// The default implementation of a component waiter.
/// </summary>
public sealed class DefaultComponentWaiter : IComponentWaiter
{
    private readonly IMemoryCache cache;
    private readonly ConcurrentDictionary<Ulid, InFlightComponentKey> inFlightComponents;
    private readonly ILogger<IComponentWaiter> logger;
    private readonly InteractivityOptions options;
    private readonly IServiceProvider serviceProvider;

    public DefaultComponentWaiter(IMemoryCache cache, ILogger<IComponentWaiter> logger, IOptions<InteractivityOptions> options, IServiceProvider services)
    {
        this.cache = cache;
        this.logger = logger;
        this.inFlightComponents = [];
        this.options = options.Value;
        this.serviceProvider = services;
    }

    /// <inheritdoc/>
    public async Task<Result<ComponentInteractionCreatedEventArgs>> WaitForComponentInteractionAsync
    (
        ulong messageId,
        IReadOnlyList<string> customIds,
        Func<ComponentInteractionCreatedEventArgs, bool> condition,
        TimeSpan timeout
    )
    {
        InFlightComponentRequest request = new(condition, new());
        InFlightComponentKey inFlightKey = new(messageId, customIds);
        Ulid key = Ulid.NewUlid();
        
        this.cache.CreateEntry(key)
            .SetAbsoluteExpiration(timeout)
            .SetValue(request)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                if (reason == EvictionReason.Expired)
                {
                    this.inFlightComponents.Remove((Ulid)key, out _);
                    ((InFlightComponentRequest)value!).CompletionSource.SetResult(new TimeoutError());
                }
            })
            .Dispose();

        this.inFlightComponents.AddOrUpdate(key, inFlightKey, (_, _) => inFlightKey);

        this.logger.LogTrace
        (
            "Registered component interaction to wait on message {messageId}:[{customIds}], tracking ID: {id}",
            messageId,
            string.Join(", ", customIds),
            key
        );

        return await request.CompletionSource.Task;
    }

    /// <inheritdoc/>
    public async Task HandleComponentInteractionAsync(DiscordClient client, ComponentInteractionCreatedEventArgs eventArgs)
    {
        KeyValuePair<Ulid, InFlightComponentKey>? kvp = this.inFlightComponents
            .FirstOrDefault(x => x.Value.MessageId == eventArgs.Message.Id && (x.Value.CustomIds.Contains(eventArgs.Id) || x.Value.CustomIds is ["*"]));

        if (!kvp.HasValue)
        {
            return;
        }

        InFlightComponentRequest? request = this.cache.Get<InFlightComponentRequest>(kvp.Value);

        if (request is null)
        {
            return;
        }

        if (eventArgs.Interaction.ResponseState != DiscordInteractionResponseState.Unacknowledged 
            && this.options.ResponseBehavior != ComponentInteractionResponseBehavior.Ignore)
        {
            this.logger.LogWarning
            (
                "An interaction with component {messageId}:{customId} was already acknowledged by other code. To prevent double responses, "
                + "ensure your code does not respond to interactions it does not handle, or configure Interactivity to ignore undesired interactions.",
                kvp.Value.Value.MessageId,
                eventArgs.Id
            );
        }

        if (!request.Condition(eventArgs))
        {
            switch (this.options.ResponseBehavior)
            {
                // we want to ignore it, explicitly ignore it
                case ComponentInteractionResponseBehavior.Ignore:
                    break;

                case ComponentInteractionResponseBehavior.Respond:

                    await eventArgs.Interaction.CreateResponseAsync
                    (
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        this.options.ResponseMessageFactory(eventArgs, this.serviceProvider).AsEphemeral()
                    );

                    break;
            }
        }
        else
        {
            request.CompletionSource.SetResult(eventArgs);

            this.cache.Remove(kvp.Value.Key);
            this.inFlightComponents.Remove(kvp.Value.Key, out _);
        }
    }

    private record struct InFlightComponentKey(ulong MessageId, IReadOnlyList<string> CustomIds);
    private record InFlightComponentRequest
    (
        Func<ComponentInteractionCreatedEventArgs, bool> Condition,
        TaskCompletionSource<Result<ComponentInteractionCreatedEventArgs>> CompletionSource
    );
}
