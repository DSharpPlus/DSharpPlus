namespace DSharpPlus.Interactivity.EventHandling;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;

internal class ButtonPaginationRequest : IPaginationRequest
{
    private int _index;
    private readonly List<Page> _pages = new();

    private readonly TaskCompletionSource<bool> _tcs = new();

    private readonly CancellationToken _token;
    private readonly DiscordUser _user;
    private readonly DiscordMessage _message;
    private readonly PaginationButtons _buttons;
    private readonly PaginationBehaviour _wrapBehavior;
    private readonly ButtonPaginationBehavior _behaviorBehavior;

    public ButtonPaginationRequest(DiscordMessage message, DiscordUser user,
        PaginationBehaviour behavior, ButtonPaginationBehavior behaviorBehavior,
        PaginationButtons buttons, IEnumerable<Page> pages, CancellationToken token)
    {
        _user = user;
        _token = token;
        _buttons = new(buttons);
        _message = message;
        _wrapBehavior = behavior;
        _behaviorBehavior = behaviorBehavior;
        _pages.AddRange(pages);

        _token.Register(() => _tcs.TrySetResult(false));
    }

    public int PageCount => _pages.Count;

    public Task<Page> GetPageAsync()
    {
        Task<Page> page = Task.FromResult(_pages[_index]);

        if (PageCount is 1)
        {
            _buttons.SkipLeft.Disable();
            _buttons.Left.Disable();
            _buttons.Right.Disable();
            _buttons.SkipRight.Disable();

            _buttons.Stop.Enable();
            return page;
        }

        if (_wrapBehavior is PaginationBehaviour.WrapAround)
        {
            return page;
        }

        _buttons.SkipLeft.Disabled = _index < 2;

        _buttons.Left.Disabled = _index < 1;

        _buttons.Right.Disabled = _index >= PageCount - 1;

        _buttons.SkipRight.Disabled = _index >= PageCount - 2;

        return page;
    }

    public Task SkipLeftAsync()
    {
        if (_wrapBehavior is PaginationBehaviour.WrapAround)
        {
            _index = _index is 0 ? _pages.Count - 1 : 0;
            return Task.CompletedTask;
        }

        _index = 0;

        return Task.CompletedTask;
    }

    public Task SkipRightAsync()
    {
        if (_wrapBehavior is PaginationBehaviour.WrapAround)
        {
            _index = _index == PageCount - 1 ? 0 : PageCount - 1;
            return Task.CompletedTask;
        }

        _index = _pages.Count - 1;

        return Task.CompletedTask;
    }

    public Task NextPageAsync()
    {
        _index++;

        if (_wrapBehavior is PaginationBehaviour.WrapAround)
        {
            if (_index >= PageCount)
            {
                _index = 0;
            }

            return Task.CompletedTask;
        }

        _index = Math.Min(_index, PageCount - 1);

        return Task.CompletedTask;
    }

    public Task PreviousPageAsync()
    {
        _index--;

        if (_wrapBehavior is PaginationBehaviour.WrapAround)
        {
            if (_index is -1)
            {
                _index = _pages.Count - 1;
            }

            return Task.CompletedTask;
        }

        _index = Math.Max(_index, 0);

        return Task.CompletedTask;
    }

    public Task<PaginationEmojis> GetEmojisAsync()
        => Task.FromException<PaginationEmojis>(new NotSupportedException("Emojis aren't supported for this request."));

    public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
        => Task.FromResult((IEnumerable<DiscordButtonComponent>)_buttons.ButtonArray);

    public Task<DiscordMessage> GetMessageAsync() => Task.FromResult(_message);

    public Task<DiscordUser> GetUserAsync() => Task.FromResult(_user);

    public Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync() => Task.FromResult(_tcs);

    // This is essentially the stop method. //
    public async Task DoCleanupAsync()
    {
        switch (_behaviorBehavior)
        {
            case ButtonPaginationBehavior.Disable:
                IEnumerable<DiscordButtonComponent> buttons = _buttons.ButtonArray.Select(b => b.Disable());

                DiscordMessageBuilder builder = new DiscordMessageBuilder()
                    .WithContent(_pages[_index].Content)
                    .AddEmbed(_pages[_index].Embed)
                    .AddComponents(buttons);

                await builder.ModifyAsync(_message);
                break;

            case ButtonPaginationBehavior.DeleteButtons:
                builder = new DiscordMessageBuilder()
                    .WithContent(_pages[_index].Content)
                    .AddEmbed(_pages[_index].Embed);

                await builder.ModifyAsync(_message);
                break;

            case ButtonPaginationBehavior.DeleteMessage:
                await _message.DeleteAsync();
                break;

            case ButtonPaginationBehavior.Ignore:
                break;
        }
    }
}
