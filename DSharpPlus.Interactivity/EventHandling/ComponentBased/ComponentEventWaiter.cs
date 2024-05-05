using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

/// <summary>
/// A component-based version of <see cref="EventWaiter{T}"/>
/// </summary>
internal class ComponentEventWaiter : IDisposable
{
    private readonly DiscordClient client;
    private readonly ConcurrentHashSet<ComponentMatchRequest> matchRequests = [];
    private readonly ConcurrentHashSet<ComponentCollectRequest> collectRequests = [];

    private readonly DiscordFollowupMessageBuilder message;
    private readonly InteractivityConfiguration config;

    public ComponentEventWaiter(DiscordClient client, InteractivityConfiguration config)
    {
        this.client = client;
        this.client.ComponentInteractionCreated += HandleAsync;
        this.config = config;

        this.message = new() { Content = config.ResponseMessage ?? "This message was not meant for you.", IsEphemeral = true };
    }

    /// <summary>
    /// Waits for a specified <see cref="ComponentMatchRequest"/>'s predicate to be fulfilled.
    /// </summary>
    /// <param name="request">The request to wait for.</param>
    /// <returns>The returned args, or null if it timed out.</returns>
    public async Task<ComponentInteractionCreateEventArgs> WaitForMatchAsync(ComponentMatchRequest request)
    {
        this.matchRequests.Add(request);

        try
        {
            return await request.Tcs.Task;
        }
        catch (Exception e)
        {
            this.client.Logger.LogError(InteractivityEvents.InteractivityWaitError, e, "An exception was thrown while waiting for components.");
            return null;
        }
        finally
        {
            this.matchRequests.TryRemove(request);
        }
    }

    /// <summary>
    /// Collects reactions and returns the result when the <see cref="ComponentMatchRequest"/>'s cancellation token is canceled.
    /// </summary>
    /// <param name="request">The request to wait on.</param>
    /// <returns>The result from request's predicate over the period of time leading up to the token's cancellation.</returns>
    public async Task<IReadOnlyList<ComponentInteractionCreateEventArgs>> CollectMatchesAsync(ComponentCollectRequest request)
    {
        this.collectRequests.Add(request);
        try
        {
            await request.Tcs.Task;
        }
        catch (Exception e)
        {
            this.client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, e, "There was an error while collecting component event args.");
        }
        finally
        {
            this.collectRequests.TryRemove(request);
        }
        return request.Collected.ToArray();
    }

    private async Task HandleAsync(DiscordClient _, ComponentInteractionCreateEventArgs args)
    {
        foreach (ComponentMatchRequest? mreq in this.matchRequests.ToArray())
        {
            if (mreq.Message == args.Message && mreq.IsMatch(args))
            {
                mreq.Tcs.TrySetResult(args);
            }
            else if (this.config.ResponseBehavior is InteractionResponseBehavior.Respond)
            {
                await args.Interaction.CreateFollowupMessageAsync(this.message);
            }
        }

        foreach (ComponentCollectRequest? creq in this.collectRequests.ToArray())
        {
            if (creq.Message == args.Message && creq.IsMatch(args))
            {
                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

                if (creq.IsMatch(args))
                {
                    creq.Collected.Add(args);
                }
                else if (this.config.ResponseBehavior is InteractionResponseBehavior.Respond)
                {
                    await args.Interaction.CreateFollowupMessageAsync(this.message);
                }
            }
        }
    }
    public void Dispose()
    {
        this.matchRequests.Clear();
        this.collectRequests.Clear();
        this.client.ComponentInteractionCreated -= HandleAsync;
    }
}
