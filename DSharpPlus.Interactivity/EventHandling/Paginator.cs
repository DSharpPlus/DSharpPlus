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
    private DiscordClient client;
    private ConcurrentHashSet<IPaginationRequest> requests;

    /// <summary>
    /// Creates a new Eventwaiter object.
    /// </summary>
    /// <param name="client">Your DiscordClient</param>
    public Paginator(DiscordClient client)
    {
        this.client = client;
        this.requests = [];
    }

    public async Task DoPaginationAsync(IPaginationRequest request)
    {
        await ResetReactionsAsync(request);
        this.requests.Add(request);
        try
        {
            TaskCompletionSource<bool> tcs = await request.GetTaskCompletionSourceAsync();
            await tcs.Task;
        }
        catch (Exception ex)
        {
            this.client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
        }
        finally
        {
            this.requests.TryRemove(request);
            try
            {
                await request.DoCleanupAsync();
            }
            catch (Exception ex)
            {
                this.client.Logger.LogError(InteractivityEvents.InteractivityPaginationError, ex, "Exception occurred while paginating");
            }
        }
    }

    internal Task HandleReactionAdd(DiscordClient client, MessageReactionAddedEventArgs eventargs)
    {
        if (this.requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (IPaginationRequest req in this.requests)
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
                            await PaginateAsync(req, eventargs.Emoji);
                        }
                        else if (eventargs.Emoji == emojis.Stop &&
                                 req is PaginationRequest paginationRequest &&
                                 paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                        {
                            await PaginateAsync(req, eventargs.Emoji);
                        }
                        else
                        {
                            await msg.DeleteReactionAsync(eventargs.Emoji, eventargs.User);
                        }
                    }
                    else if (eventargs.User.Id != this.client.CurrentUser.Id)
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

    internal Task HandleReactionRemove(DiscordClient client, MessageReactionRemovedEventArgs eventargs)
    {
        if (this.requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (IPaginationRequest req in this.requests)
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
                            await PaginateAsync(req, eventargs.Emoji);
                        }
                        else if (eventargs.Emoji == emojis.Stop &&
                                 req is PaginationRequest paginationRequest &&
                                 paginationRequest.PaginationDeletion == PaginationDeletion.DeleteMessage)
                        {
                            await PaginateAsync(req, eventargs.Emoji);
                        }
                    }
                }
            }
        });

        return Task.CompletedTask;
    }

    internal Task HandleReactionClear(DiscordClient client, MessageReactionsClearedEventArgs eventargs)
    {
        if (this.requests.Count == 0)
        {
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {
            foreach (IPaginationRequest req in this.requests)
            {
                DiscordMessage msg = await req.GetMessageAsync();

                if (msg.Id == eventargs.Message.Id)
                {
                    await ResetReactionsAsync(req);
                }
            }
        });

        return Task.CompletedTask;
    }

    private static async Task ResetReactionsAsync(IPaginationRequest p)
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

        if (mbr != null /* == is guild and cache is valid */ && (chn.PermissionsFor(mbr) & DiscordPermissions.ManageChannels) != 0) /* == has permissions */
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

    private static async Task PaginateAsync(IPaginationRequest p, DiscordEmoji emoji)
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

        this.requests?.Clear();
        this.requests = null!;
    }
}
