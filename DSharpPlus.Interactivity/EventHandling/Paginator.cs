using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Concurrency;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling
{
    internal class Paginator
    {
        DiscordClient _client;
        ConcurrentHashSet<IPaginationRequest> _requests;

        /// <summary>
        /// Creates a new Eventwaiter object.
        /// </summary>
        /// <param name="client">Your DiscordClient</param>
        public Paginator(DiscordClient client)
        {
            this._client = client;
            this._requests = new ConcurrentHashSet<IPaginationRequest>();

            this._client.MessageReactionAdded += this.HandleReactionAdd;
            this._client.MessageReactionRemoved += this.HandleReactionRemove;
            this._client.MessageReactionsCleared += this.HandleReactionClear;
        }

        public async Task DoPaginationAsync(IPaginationRequest request)
        {
            await ResetReactionsAsync(request);
            this._requests.Add(request);
            try
            {
                var tcs = await request.GetTaskCompletionSourceAsync();
                await tcs.Task;
                await request.DoCleanupAsync();
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
            }
            finally
            {
                this._requests.TryRemove(request);
            }
        }

        async Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                var emojis = await req.GetEmojisAsync();
                var msg = await req.GetMessageAsync();
                var usr = await req.GetUserAsync();

                if (msg.Id == eventargs.Message.Id)
                {
                    if (eventargs.User.Id == usr.Id)
                    {
                        await PaginateAsync(req, eventargs.Emoji);
                    }
                    else if (eventargs.User.Id != _client.CurrentUser.Id)
                    {
                        if (eventargs.Emoji != emojis.Left &&
                           eventargs.Emoji != emojis.SkipLeft &&
                           eventargs.Emoji != emojis.Right &&
                           eventargs.Emoji != emojis.SkipRight &&
                           eventargs.Emoji != emojis.Stop)
                        {
                            await msg.DeleteReactionAsync(eventargs.Emoji, eventargs.User);
                        }
                    }
                }
            }
        }

        async Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                var emojis = await req.GetEmojisAsync();
                var msg = await req.GetMessageAsync();
                var usr = await req.GetUserAsync();

                if (msg.Id == eventargs.Message.Id)
                {
                    if (eventargs.User.Id == usr.Id)
                    {
                        await PaginateAsync(req, eventargs.Emoji);
                    }
                    if (eventargs.Emoji != emojis.Left &&
                           eventargs.Emoji != emojis.SkipLeft &&
                           eventargs.Emoji != emojis.Right &&
                           eventargs.Emoji != emojis.SkipRight &&
                           eventargs.Emoji != emojis.Stop)
                    {
                        await msg.DeleteReactionAsync(eventargs.Emoji, eventargs.User);
                    }
                }
            }
        }

        async Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
        {
            await Task.Yield();
            foreach (var req in _requests)
            {
                var msg = await req.GetMessageAsync();

                if (msg.Id == eventargs.Message.Id)
                {
                    await ResetReactionsAsync(req);
                }
            }
        }

        async Task ResetReactionsAsync(IPaginationRequest p)
        {
            var msg = await p.GetMessageAsync();
            var emojis = await p.GetEmojisAsync();

            // Test permissions to avoid a 403:
            // https://totally-not.a-sketchy.site/3pXpRLK.png
            // Yes, this is an issue
            // No, we should not require people to guarantee MANAGE_MESSAGES
            // Need to check following:
            // - In guild?
            //  - If yes, check if have permission
            // - If all above fail (DM || guild && no permission), skip this
            var chn = msg.Channel;
            var gld = chn?.Guild;
            var mbr = gld?.CurrentMember;

            if (mbr != null /* == is guild and cache is valid */ && (chn.PermissionsFor(mbr) & Permissions.ManageChannels) != 0) /* == has permissions */
                await msg.DeleteAllReactionsAsync("Pagination");
            // ENDOF: 403 fix

            if (emojis.SkipLeft != null)
                await msg.CreateReactionAsync(emojis.SkipLeft);
            if (emojis.Left != null)
                await msg.CreateReactionAsync(emojis.Left);
            if (emojis.Right != null)
                await msg.CreateReactionAsync(emojis.Right);
            if (emojis.SkipRight != null)
                await msg.CreateReactionAsync(emojis.SkipRight);
            if (emojis.Stop != null)
                await msg.CreateReactionAsync(emojis.Stop);
        }

        async Task PaginateAsync(IPaginationRequest p, DiscordEmoji emoji)
        {
            var emojis = await p.GetEmojisAsync();
            var msg = await p.GetMessageAsync();

            if (emoji == emojis.SkipLeft)
                await p.SkipLeftAsync();
            else if (emoji == emojis.Left)
                await p.PreviousPageAsync();
            else if (emoji == emojis.Right)
                await p.NextPageAsync();
            else if (emoji == emojis.SkipRight)
                await p.SkipRightAsync();
            else if (emoji == emojis.Stop)
            {
                var tcs = await p.GetTaskCompletionSourceAsync();
                tcs.TrySetResult(true);
                return;
            }

            var page = await p.GetPageAsync();
            await msg.ModifyAsync(page.Content, page.Embed);
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
