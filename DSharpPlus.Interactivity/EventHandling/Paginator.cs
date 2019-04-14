using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
    internal class Paginator
    {
        DiscordClient _client;
        ConcurrentHashSet<PaginationRequest> _requests;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="client">Your DiscordClient</param>
        public Paginator(DiscordClient client)
        {
            this._client = client;
            this._requests = new ConcurrentHashSet<PaginationRequest>();

            this._client.MessageReactionAdded += this.HandleReactionAdd;
            this._client.MessageReactionRemoved += this.HandleReactionRemove;
            this._client.MessageReactionsCleared += this.HandleReactionClear;
        }

        public async Task DoPaginationAsync(PaginationRequest request)
        {
            await ResetReactionsAsync(request);
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
                request.Dispose();
                this._requests.TryRemove(request);
            }
        }

        async Task HandleReactionAdd(MessageReactionAddEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                if (req._message.Id == eventargs.Message.Id)
                {
                    if (eventargs.User.Id == req._user.Id)
                    {
                        await PaginateAsync(req, eventargs.Emoji);
                    }
                    else if (eventargs.User.Id != _client.CurrentUser.Id)
                    {
                        if (eventargs.Emoji != req._emojis.Left ||
                           eventargs.Emoji != req._emojis.SkipLeft ||
                           eventargs.Emoji != req._emojis.Right ||
                           eventargs.Emoji != req._emojis.SkipRight ||
                           eventargs.Emoji != req._emojis.Stop)
                            await ResetReactionsAsync(req);
                    }
                }
            }
        }

        async Task HandleReactionRemove(MessageReactionRemoveEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                if (req._message.Id == eventargs.Message.Id)
                {
                    if (eventargs.User.Id == req._user.Id)
                    {
                        await PaginateAsync(req, eventargs.Emoji);
                    }
                    else if(eventargs.User.Id != _client.CurrentUser.Id)
                    {
                        if (eventargs.Emoji != req._emojis.Left ||
                           eventargs.Emoji != req._emojis.SkipLeft ||
                           eventargs.Emoji != req._emojis.Right ||
                           eventargs.Emoji != req._emojis.SkipRight ||
                           eventargs.Emoji != req._emojis.Stop)
                            await ResetReactionsAsync(req);
                    }
                }
            }
        }

        async Task HandleReactionClear(MessageReactionsClearEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                if(req._message.Id == eventargs.Message.Id)
                {
                    await ResetReactionsAsync(req);
                }
            }
        }

        async Task PaginateAsync(PaginationRequest p, DiscordEmoji emoji)
        {
            if (emoji == p._emojis.SkipLeft)
                p.SkipLeft();
            else if (emoji == p._emojis.Left)
                p.PreviousPage();
            else if (emoji == p._emojis.Right)
                p.NextPage();
            else if (emoji == p._emojis.SkipRight)
                p.SkipRight();
            else if (emoji == p._emojis.Stop)
            {
                p._tcs.TrySetResult(true);
                return;
            }

            var page = p.GetPage();
            await p._message.ModifyAsync(page.Content, page.Embed);
        }

        async Task ResetReactionsAsync(PaginationRequest p)
        {
            await p._message.DeleteAllReactionsAsync("Pagination");

            if (p._emojis.SkipLeft != null)
                await p._message.CreateReactionAsync(p._emojis.SkipLeft);
            if (p._emojis.Left != null)
                await p._message.CreateReactionAsync(p._emojis.Left);
            if (p._emojis.Right != null)
                await p._message.CreateReactionAsync(p._emojis.Right);
            if (p._emojis.SkipRight != null)
                await p._message.CreateReactionAsync(p._emojis.SkipRight);
            if (p._emojis.Stop != null)
                await p._message.CreateReactionAsync(p._emojis.Stop);
        }

        ~Paginator()
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
