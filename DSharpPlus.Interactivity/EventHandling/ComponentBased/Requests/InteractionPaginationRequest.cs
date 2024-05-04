using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;

namespace DSharpPlus.Interactivity.EventHandling;

internal class InteractionPaginationRequest : IPaginationRequest
{
    private int index;
    private readonly List<Page> pages = [];

    private readonly TaskCompletionSource<bool> tcs = new();

    private DiscordInteraction lastInteraction;
    private CancellationTokenSource interactionCts;

    private readonly CancellationToken token;
    private readonly DiscordUser user;
    private readonly DiscordMessage message;
    private readonly PaginationButtons buttons;
    private readonly PaginationBehaviour wrapBehavior;
    private readonly ButtonPaginationBehavior behaviorBehavior;

    public InteractionPaginationRequest(DiscordInteraction interaction, DiscordMessage message, DiscordUser user,
        PaginationBehaviour behavior, ButtonPaginationBehavior behaviorBehavior,
        PaginationButtons buttons, IEnumerable<Page> pages, CancellationToken token)
    {
        this.user = user;
        this.token = token;
        this.buttons = new(buttons);
        this.message = message;
        this.wrapBehavior = behavior;
        this.behaviorBehavior = behaviorBehavior;
        this.pages.AddRange(pages);

        this.RegenerateCTS(interaction);
        this.token.Register(() => this.tcs.TrySetResult(false));
    }

    public int PageCount => this.pages.Count;

    internal void RegenerateCTS(DiscordInteraction interaction)
    {
        this.interactionCts?.Dispose();
        this.lastInteraction = interaction;
        this.interactionCts = new(TimeSpan.FromSeconds((60 * 15) - 5));
        this.interactionCts.Token.Register(() => this.tcs.TrySetResult(false));
    }

    public Task<Page> GetPageAsync()
    {
        Task<Page> page = Task.FromResult(this.pages[this.index]);

        if (this.PageCount is 1)
        {
            foreach (DiscordButtonComponent button in this.buttons.ButtonArray)
            {
                button.Disable();
            }

            this.buttons.Stop.Enable();
            return page;
        }

        if (this.wrapBehavior is PaginationBehaviour.WrapAround)
        {
            return page;
        }

        this.buttons.SkipLeft.Disabled = this.index < 2;

        this.buttons.Left.Disabled = this.index < 1;

        this.buttons.Right.Disabled = this.index == this.PageCount - 1;

        this.buttons.SkipRight.Disabled = this.index >= this.PageCount - 2;

        return page;
    }

    public Task SkipLeftAsync()
    {
        if (this.wrapBehavior is PaginationBehaviour.WrapAround)
        {
            this.index = this.index is 0 ? this.pages.Count - 1 : 0;
            return Task.CompletedTask;
        }

        this.index = 0;

        return Task.CompletedTask;
    }

    public Task SkipRightAsync()
    {
        if (this.wrapBehavior is PaginationBehaviour.WrapAround)
        {
            this.index = this.index == this.PageCount - 1 ? 0 : this.PageCount - 1;
            return Task.CompletedTask;
        }

        this.index = this.pages.Count - 1;

        return Task.CompletedTask;
    }

    public Task NextPageAsync()
    {
        this.index++;

        if (this.wrapBehavior is PaginationBehaviour.WrapAround)
        {
            if (this.index >= this.PageCount)
            {
                this.index = 0;
            }

            return Task.CompletedTask;
        }

        this.index = Math.Min(this.index, this.PageCount - 1);

        return Task.CompletedTask;
    }

    public Task PreviousPageAsync()
    {
        this.index--;

        if (this.wrapBehavior is PaginationBehaviour.WrapAround)
        {
            if (this.index is -1)
            {
                this.index = this.pages.Count - 1;
            }

            return Task.CompletedTask;
        }

        this.index = Math.Max(this.index, 0);

        return Task.CompletedTask;
    }

    public Task<PaginationEmojis> GetEmojisAsync()
        => Task.FromException<PaginationEmojis>(new NotSupportedException("Emojis aren't supported for this request."));

    public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
        => Task.FromResult((IEnumerable<DiscordButtonComponent>)this.buttons.ButtonArray);

    public Task<DiscordMessage> GetMessageAsync() => Task.FromResult(this.message);

    public Task<DiscordUser> GetUserAsync() => Task.FromResult(this.user);

    public Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync() => Task.FromResult(this.tcs);

    // This is essentially the stop method. //
    public async Task DoCleanupAsync()
    {
        switch (this.behaviorBehavior)
        {
            case ButtonPaginationBehavior.Disable:
                IEnumerable<DiscordButtonComponent> buttons = this.buttons.ButtonArray
                    .Select(b => new DiscordButtonComponent(b))
                    .Select(b => b.Disable());

                DiscordWebhookBuilder builder = new DiscordWebhookBuilder()
                    .WithContent(this.pages[this.index].Content)
                    .AddEmbed(this.pages[this.index].Embed)
                    .AddComponents(buttons);

                await this.lastInteraction.EditOriginalResponseAsync(builder);
                break;

            case ButtonPaginationBehavior.DeleteButtons:
                builder = new DiscordWebhookBuilder()
                    .WithContent(this.pages[this.index].Content)
                    .AddEmbed(this.pages[this.index].Embed);

                await this.lastInteraction.EditOriginalResponseAsync(builder);
                break;

            case ButtonPaginationBehavior.DeleteMessage:
                await this.lastInteraction.DeleteOriginalResponseAsync();
                break;

            case ButtonPaginationBehavior.Ignore:
                break;
        }
    }
}
