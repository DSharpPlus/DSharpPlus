using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

internal class Poller
{
    private DiscordClient client;
    private ConcurrentHashSet<PollRequest> requests;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="client">Your DiscordClient</param>
    public Poller(DiscordClient client)
    {
        this.client = client;
        this.requests = [];

        this.client.MessageReactionAdded += HandleReactionAdd;
        this.client.MessageReactionRemoved += HandleReactionRemove;
        this.client.MessageReactionsCleared += HandleReactionClear;
    }

    public async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(PollRequest request)
    {
        ReadOnlyCollection<PollEmoji> result;
        this.requests.Add(request);
        try
        {
            await request.tcs.Task;
        }
        catch (Exception ex)
        {
            this.client.Logger.LogError(InteractivityEvents.InteractivityPollError, ex, "Exception occurred while polling");
        }
        finally
        {
            result = new ReadOnlyCollection<PollEmoji>(new HashSet<PollEmoji>(request.collected).ToList());
            request.Dispose();
            this.requests.TryRemove(request);
        }
        return result;
    }

    private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
    {
        if (this.requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (PollRequest req in this.requests)
            {
                // match message
                if (req.message.Id == eventargs.Message.Id && req.message.ChannelId == eventargs.Channel.Id)
                {
                    if (req.emojis.Contains(eventargs.Emoji) && !req.collected.Any(x => x.Voted.Contains(eventargs.User)))
                    {
                        if (eventargs.User.Id != this.client.CurrentUser.Id)
                        {
                            req.AddReaction(eventargs.Emoji, eventargs.User);
                        }
                    }
                    else
                    {
                        Entities.DiscordMember member = await eventargs.Channel.Guild.GetMemberAsync(client.CurrentUser.Id);
                        if (eventargs.Channel.PermissionsFor(member).HasPermission(DiscordPermissions.ManageMessages))
                        {
                            await eventargs.Message.DeleteReactionAsync(eventargs.Emoji, eventargs.User);
                        }
                    }
                }
            }
        });
        return Task.CompletedTask;
    }

    private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventargs)
    {
        foreach (PollRequest req in this.requests)
        {
            // match message
            if (req.message.Id == eventargs.Message.Id && req.message.ChannelId == eventargs.Channel.Id)
            {
                if (eventargs.User.Id != this.client.CurrentUser.Id)
                {
                    req.RemoveReaction(eventargs.Emoji, eventargs.User);
                }
            }
        }
        return Task.CompletedTask;
    }

    private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
    {
        foreach (PollRequest req in this.requests)
        {
            // match message
            if (req.message.Id == eventargs.Message.Id && req.message.ChannelId == eventargs.Channel.Id)
            {
                req.ClearCollected();
            }
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes this EventWaiter
    /// </summary>
    public void Dispose()
    {
        // Why doesn't this class implement IDisposable?

        if (this.client != null)
        {
            this.client.MessageReactionAdded -= HandleReactionAdd;
            this.client.MessageReactionRemoved -= HandleReactionRemove;
            this.client.MessageReactionsCleared -= HandleReactionClear;
            this.client = null!;
        }

        if (this.requests != null)
        {
            this.requests.Clear();
            this.requests = null!;
        }
    }
}
