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
    private readonly DiscordClient _client;
    private readonly ConcurrentHashSet<ComponentMatchRequest> _matchRequests = new();
    private readonly ConcurrentHashSet<ComponentCollectRequest> _collectRequests = new();

    private readonly DiscordFollowupMessageBuilder _message;
    private readonly InteractivityConfiguration _config;

    public ComponentEventWaiter(DiscordClient client, InteractivityConfiguration config)
    {
        _client = client;
        _client.ComponentInteractionCreated += Handle;
        _config = config;

        _message = new() { Content = config.ResponseMessage ?? "This message was not meant for you.", IsEphemeral = true };
    }

    /// <summary>
    /// Waits for a specified <see cref="ComponentMatchRequest"/>'s predicate to be fulfilled.
    /// </summary>
    /// <param name="request">The request to wait for.</param>
    /// <returns>The returned args, or null if it timed out.</returns>
    public async Task<ComponentInteractionCreateEventArgs> WaitForMatchAsync(ComponentMatchRequest request)
    {
        _matchRequests.Add(request);

        try
        {
            return await request.Tcs.Task;
        }
        catch (Exception e)
        {
            _client.Logger.LogError(InteractivityEvents.InteractivityWaitError, e, "An exception was thrown while waiting for components.");
            return null;
        }
        finally
        {
            _matchRequests.TryRemove(request);
        }
    }

    /// <summary>
    /// Collects reactions and returns the result when the <see cref="ComponentMatchRequest"/>'s cancellation token is canceled.
    /// </summary>
    /// <param name="request">The request to wait on.</param>
    /// <returns>The result from request's predicate over the period of time leading up to the token's cancellation.</returns>
    public async Task<IReadOnlyList<ComponentInteractionCreateEventArgs>> CollectMatchesAsync(ComponentCollectRequest request)
    {
        _collectRequests.Add(request);
        try
        {
            await request.Tcs.Task;
        }
        catch (Exception e)
        {
            _client.Logger.LogError(InteractivityEvents.InteractivityCollectorError, e, "There was an error while collecting component event args.");
        }
        finally
        {
            _collectRequests.TryRemove(request);
        }
        return request.Collected.ToArray();
    }

    private async Task Handle(DiscordClient _, ComponentInteractionCreateEventArgs args)
    {
        foreach (ComponentMatchRequest? mreq in _matchRequests.ToArray())
        {
            if (mreq.Message == args.Message && mreq.IsMatch(args))
            {
                mreq.Tcs.TrySetResult(args);
            }
            else if (_config.ResponseBehavior is InteractionResponseBehavior.Respond)
            {
                await args.Interaction.CreateFollowupMessageAsync(_message);
            }
        }


        foreach (ComponentCollectRequest? creq in _collectRequests.ToArray())
        {
            if (creq.Message == args.Message && creq.IsMatch(args))
            {
                await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

                if (creq.IsMatch(args))
                {
                    creq.Collected.Add(args);
                }
                else if (_config.ResponseBehavior is InteractionResponseBehavior.Respond)
                {
                    await args.Interaction.CreateFollowupMessageAsync(_message);
                }
            }
        }
    }
    public void Dispose()
    {
        _matchRequests.Clear();
        _collectRequests.Clear();
        _client.ComponentInteractionCreated -= Handle;
    }
}
