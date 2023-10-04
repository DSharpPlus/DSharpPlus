using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

internal class Poller
{
    DiscordClient _client;
    ConcurrentHashSet<PollRequest> _requests;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="client">Your DiscordClient</param>
    public Poller(DiscordClient client)
    {
        this._client = client;
        this._requests = new ConcurrentHashSet<PollRequest>();

        this._client.MessageReactionAdded += this.HandleReactionAdd;
        this._client.MessageReactionRemoved += this.HandleReactionRemove;
        this._client.MessageReactionsCleared += this.HandleReactionClear;
    }

    public async Task<ReadOnlyCollection<PollEmoji>> DoPollAsync(PollRequest request)
    {
        ReadOnlyCollection<PollEmoji> result = null;
        this._requests.Add(request);
        try
        {
            await request._tcs.Task;
        }
        catch (Exception ex)
        {
            this._client.Logger.LogError(InteractivityEvents.InteractivityPollError, ex, "Exception occurred while polling");
        }
        finally
        {
            result = new ReadOnlyCollection<PollEmoji>(new HashSet<PollEmoji>(request._collected).ToList());
            request.Dispose();
            this._requests.TryRemove(request);
        }
        return result;
    }

    private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
    {
        if (this._requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (PollRequest req in this._requests)
            {
                // match message
                if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
                {
                    if (req._emojis.Contains(eventargs.Emoji) && !req._collected.Any(x => x.Voted.Contains(eventargs.User)))
                    {
                        if (eventargs.User.Id != this._client.CurrentUser.Id)
                        {
                            req.AddReaction(eventargs.Emoji, eventargs.User);
                        }
                    }
                    else
                    {
                        Entities.DiscordMember member = await eventargs.Channel.Guild.GetMemberAsync(client.CurrentUser.Id);
                        if (eventargs.Channel.PermissionsFor(member).HasPermission(Permissions.ManageMessages))
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
        foreach (PollRequest req in this._requests)
        {
            // match message
            if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
            {
                if (eventargs.User.Id != this._client.CurrentUser.Id)
                {
                    req.RemoveReaction(eventargs.Emoji, eventargs.User);
                }
            }
        }
        return Task.CompletedTask;
    }

    private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
    {
        foreach (PollRequest req in this._requests)
        {
            // match message
            if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
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

        if (this._client != null)
        {
            this._client.MessageReactionAdded -= this.HandleReactionAdd;
            this._client.MessageReactionRemoved -= this.HandleReactionRemove;
            this._client.MessageReactionsCleared -= this.HandleReactionClear;
            this._client = null!;
        }

        if (this._requests != null)
        {
            this._requests.Clear();
            this._requests = null!;
        }
    }
}
