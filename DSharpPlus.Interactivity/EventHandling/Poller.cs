using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Concurrency;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
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
                this._client.DebugLogger.LogMessage(LogLevel.Error, "Interactivity",
                    $"Something went wrong with exception {ex.GetType().Name}.", DateTime.Now);
            }
            finally
            {
                result = new ReadOnlyCollection<PollEmoji>(new HashSet<PollEmoji>(request._collected).ToList());
                request.Dispose();
                this._requests.TryRemove(request);
            }
            return result;
        }

        async Task HandleReactionAdd(MessageReactionAddEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                // match message
                if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
                {
                    if (req._emojis.Contains(eventargs.Emoji) && !req._collected.Any(x => x.Voted.Contains(eventargs.User)))
                    {
                        if (eventargs.User.Id != _client.CurrentUser.Id)
                            req.AddReaction(eventargs.Emoji, eventargs.User);
                    }
                    else
                    {
                        var member = await eventargs.Channel.Guild.GetMemberAsync(eventargs.Client.CurrentUser.Id);
                        if(eventargs.Channel.PermissionsFor(member).HasPermission(Permissions.ManageMessages))
                            await eventargs.Message.DeleteReactionAsync(eventargs.Emoji, eventargs.User);
                    }
                }
            }
        }

        async Task HandleReactionRemove(MessageReactionRemoveEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                // match message
                if (req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
                {
                    if(eventargs.User.Id != _client.CurrentUser.Id)
                        req.RemoveReaction(eventargs.Emoji, eventargs.User);
                }
            }
        }

        async Task HandleReactionClear(MessageReactionsClearEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                // match message
                if(req._message.Id == eventargs.Message.Id && req._message.ChannelId == eventargs.Channel.Id)
                {
                    req.ClearCollected();
                }
            }
        }

        ~Poller()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this EventWaiter
        /// </summary>
        public void Dispose()
        {
            this._client.MessageReactionAdded -= this.HandleReactionAdd;
            this._client.MessageReactionRemoved -= this.HandleReactionRemove;
            this._client.MessageReactionsCleared -= this.HandleReactionClear;
            this._client = null;
            this._requests.Clear();
            this._requests = null;
        }
    }
}
