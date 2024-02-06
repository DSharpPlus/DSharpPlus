using System;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity.EventHandling;

internal class Paginator : IPaginator
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
        await this.ResetReactionsAsync(request);
        this._requests.Add(request);
        try
        {
            TaskCompletionSource<bool> tcs = await request.GetTaskCompletionSourceAsync();
            await tcs.Task;
        }
        catch (Exception ex)
        {
            this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
        }
        finally
        {
            this._requests.TryRemove(request);
            try
            {
                await request.DoCleanupAsync();
            }
            catch (Exception ex)
            {
                this._client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
            }
        }
    }

    private Task HandleReactionAdd(DiscordClient client, MessageReactionAddEventArgs eventargs)
    {
        if (this._requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (IPaginationRequest req in this._requests)
            {
                PaginationEmojis emojis = await req.GetEmojisAsync();
                DiscordMessage msg = await req.GetMessageAsync();
                DiscordUser usr = await req.GetUserAsync();

                if (msg.Id == eventargs.Message.Id)
                {
                    if (eventargs.User.Id == usr.Id)
                    {
                        if (req.PageCount > 1 &&
                            (eventargs.Emoji == emojis.Left ||
                             eventargs.Emoji == emojis.SkipLeft ||
                             eventargs.Emoji == emojis.Right ||
                             eventargs.Emoji == emojis.SkipRight ||
                             eventargs.Emoji == emojis.Stop))
                        {
                            await this.PaginateAsync(req, eventargs.Emoji);
                        }
                        else if (eventargs.Emoji == emojis.Stop &&
                                 req is PaginationRequest paginationRequest &&
                                 paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                        {
                            await this.PaginateAsync(req, eventargs.Emoji);
                        }
                        else
                        {
                            await msg.DeleteReactionAsync(eventargs.Emoji, eventargs.User);
                        }
                    }
                    else if (eventargs.User.Id != this._client.CurrentUser.Id)
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
        });
        return Task.CompletedTask;
    }

    private Task HandleReactionRemove(DiscordClient client, MessageReactionRemoveEventArgs eventargs)
    {
        if (this._requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (IPaginationRequest req in this._requests)
            {
                PaginationEmojis emojis = await req.GetEmojisAsync();
                DiscordMessage msg = await req.GetMessageAsync();
                DiscordUser usr = await req.GetUserAsync();

                if (msg.Id == eventargs.Message.Id)
                {
                    if (eventargs.User.Id == usr.Id)
                    {
                        if (req.PageCount > 1 &&
                            (eventargs.Emoji == emojis.Left ||
                             eventargs.Emoji == emojis.SkipLeft ||
                             eventargs.Emoji == emojis.Right ||
                             eventargs.Emoji == emojis.SkipRight ||
                             eventargs.Emoji == emojis.Stop))
                        {
                            await this.PaginateAsync(req, eventargs.Emoji);
                        }
                        else if (eventargs.Emoji == emojis.Stop &&
                                 req is PaginationRequest paginationRequest &&
                                 paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                        {
                            await this.PaginateAsync(req, eventargs.Emoji);
                        }
                    }
                }
            }
        });

        return Task.CompletedTask;
    }

    private Task HandleReactionClear(DiscordClient client, MessageReactionsClearEventArgs eventargs)
    {
        if (this._requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (IPaginationRequest req in this._requests)
            {
                DiscordMessage msg = await req.GetMessageAsync();

                if (msg.Id == eventargs.Message.Id)
                {
                    await this.ResetReactionsAsync(req);
                }
            }
        });

        return Task.CompletedTask;
    }

    private async Task ResetReactionsAsync(IPaginationRequest p)
    {
        DiscordMessage msg = await p.GetMessageAsync();
        PaginationEmojis emojis = await p.GetEmojisAsync();

        // Test permissions to avoid a 403:
        // https://totally-not.a-sketchy.site/3pXpRLK.png
        // Yes, this is an issue
        // No, we should not require people to guarantee MANAGE_MESSAGES
        // Need to check following:
        // - In guild?
        //  - If yes, check if have permission
        // - If all above fail (DM || guild && no permission), skip this
        DiscordChannel? chn = msg.Channel;
        DiscordGuild? gld = chn?.Guild;
        DiscordMember? mbr = gld?.CurrentMember;

        if (mbr != null /* == is guild and cache is valid */ && (chn.PermissionsFor(mbr) & Permissions.ManageChannels) != 0) /* == has permissions */
        {
            await msg.DeleteAllReactionsAsync("Pagination");
        }
        // ENDOF: 403 fix

        if (p.PageCount > 1)
        {
            if (emojis.SkipLeft != null)
            {
                await msg.CreateReactionAsync(emojis.SkipLeft);
            }

            if (emojis.Left != null)
            {
                await msg.CreateReactionAsync(emojis.Left);
            }

            if (emojis.Right != null)
            {
                await msg.CreateReactionAsync(emojis.Right);
            }

            if (emojis.SkipRight != null)
            {
                await msg.CreateReactionAsync(emojis.SkipRight);
            }

            if (emojis.Stop != null)
            {
                await msg.CreateReactionAsync(emojis.Stop);
            }
        }
        else if (emojis.Stop != null && p is PaginationRequest paginationRequest && paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
        {
            await msg.CreateReactionAsync(emojis.Stop);
        }
    }

    private async Task PaginateAsync(IPaginationRequest p, DiscordEmoji emoji)
    {
        PaginationEmojis emojis = await p.GetEmojisAsync();
        DiscordMessage msg = await p.GetMessageAsync();

        if (emoji == emojis.SkipLeft)
        {
            await p.SkipLeftAsync();
        }
        else if (emoji == emojis.Left)
        {
            await p.PreviousPageAsync();
        }
        else if (emoji == emojis.Right)
        {
            await p.NextPageAsync();
        }
        else if (emoji == emojis.SkipRight)
        {
            await p.SkipRightAsync();
        }
        else if (emoji == emojis.Stop)
        {
            TaskCompletionSource<bool> tcs = await p.GetTaskCompletionSourceAsync();
            tcs.TrySetResult(true);
            return;
        }

        Page page = await p.GetPageAsync();
        DiscordMessageBuilder builder = new DiscordMessageBuilder()
            .WithContent(page.Content)
            .AddEmbed(page.Embed);

        await builder.ModifyAsync(msg);
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

        this._requests?.Clear();
        this._requests = null!;
    }
}
