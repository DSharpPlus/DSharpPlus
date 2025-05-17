using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;

namespace DSharpPlus.Interactivity.EventHandling;

internal class PaginationRequest : IPaginationRequest
{
    private TaskCompletionSource<bool> tcs;
    private readonly CancellationTokenSource ct;
    private readonly List<Page> pages;
    private readonly PaginationBehaviour behaviour;
    private readonly DiscordMessage message;
    private readonly PaginationEmojis emojis;
    private readonly DiscordUser user;
    private int index = 0;

    /// <summary>
    /// Creates a new Pagination request
    /// </summary>
    /// <param name="message">Message to paginate</param>
    /// <param name="user">User to allow control for</param>
    /// <param name="behaviour">Behaviour during pagination</param>
    /// <param name="deletion">Behavior on pagination end</param>
    /// <param name="emojis">Emojis for this pagination object</param>
    /// <param name="timeout">Timeout time</param>
    /// <param name="pages">Pagination pages</param>
    internal PaginationRequest(DiscordMessage message, DiscordUser user, PaginationBehaviour behaviour, PaginationDeletion deletion,
        PaginationEmojis emojis, TimeSpan timeout, params Page[] pages)
    {
        this.tcs = new();
        this.ct = new(timeout);
        this.ct.Token.Register(() => this.tcs.TrySetResult(true));

        this.message = message;
        this.user = user;

        this.PaginationDeletion = deletion;
        this.behaviour = behaviour;
        this.emojis = emojis;

        this.pages = [.. pages];
    }

    public int PageCount => this.pages.Count;

    public PaginationDeletion PaginationDeletion { get; }

    public async Task<Page> GetPageAsync()
    {
        await Task.Yield();

        return this.pages[this.index];
    }

    public async Task SkipLeftAsync()
    {
        await Task.Yield();

        this.index = 0;
    }

    public async Task SkipRightAsync()
    {
        await Task.Yield();

        this.index = this.pages.Count - 1;
    }

    public async Task NextPageAsync()
    {
        await Task.Yield();

        switch (this.behaviour)
        {
            case PaginationBehaviour.Ignore:
                if (this.index == this.pages.Count - 1)
                {
                    break;
                }
                else
                {
                    this.index++;
                }

                break;

            case PaginationBehaviour.WrapAround:
                if (this.index == this.pages.Count - 1)
                {
                    this.index = 0;
                }
                else
                {
                    this.index++;
                }

                break;
        }
    }

    public async Task PreviousPageAsync()
    {
        await Task.Yield();

        switch (this.behaviour)
        {
            case PaginationBehaviour.Ignore:
                if (this.index == 0)
                {
                    break;
                }
                else
                {
                    this.index--;
                }

                break;

            case PaginationBehaviour.WrapAround:
                if (this.index == 0)
                {
                    this.index = this.pages.Count - 1;
                }
                else
                {
                    this.index--;
                }

                break;
        }
    }

    public async Task<PaginationEmojis> GetEmojisAsync()
    {
        await Task.Yield();

        return this.emojis;
    }

    public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
        => throw new NotSupportedException("This request does not support buttons.");

    public async Task<DiscordMessage> GetMessageAsync()
    {
        await Task.Yield();

        return this.message;
    }

    public async Task<DiscordUser> GetUserAsync()
    {
        await Task.Yield();

        return this.user;
    }

    public async Task DoCleanupAsync()
    {
        switch (this.PaginationDeletion)
        {
            case PaginationDeletion.DeleteEmojis:
                await this.message.DeleteAllReactionsAsync();
                break;

            case PaginationDeletion.DeleteMessage:
                await this.message.DeleteAsync();
                break;

            case PaginationDeletion.KeepEmojis:
                break;
        }
    }

    public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
    {
        await Task.Yield();

        return this.tcs;
    }

    /// <summary>
    /// Disposes this PaginationRequest.
    /// </summary>
    public void Dispose()
    {
        // Why doesn't this class implement IDisposable?

        this.ct?.Dispose();
        this.tcs = null!;
    }
}
