
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;

namespace DSharpPlus.Interactivity.EventHandling;
internal class PaginationRequest : IPaginationRequest
{
    private TaskCompletionSource<bool> _tcs;
    private readonly CancellationTokenSource _ct;
    private readonly List<Page> _pages;
    private readonly PaginationBehaviour _behaviour;
    private readonly DiscordMessage _message;
    private readonly PaginationEmojis _emojis;
    private readonly DiscordUser _user;
    private int _index = 0;

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
        _tcs = new();
        _ct = new(timeout);
        _ct.Token.Register(() => _tcs.TrySetResult(true));

        _message = message;
        _user = user;

        PaginationDeletion = deletion;
        _behaviour = behaviour;
        _emojis = emojis;

        _pages = [.. pages];
    }

    public int PageCount => _pages.Count;

    public PaginationDeletion PaginationDeletion { get; }

    public async Task<Page> GetPageAsync()
    {
        await Task.Yield();

        return _pages[_index];
    }

    public async Task SkipLeftAsync()
    {
        await Task.Yield();

        _index = 0;
    }

    public async Task SkipRightAsync()
    {
        await Task.Yield();

        _index = _pages.Count - 1;
    }

    public async Task NextPageAsync()
    {
        await Task.Yield();

        switch (_behaviour)
        {
            case PaginationBehaviour.Ignore:
                if (_index == _pages.Count - 1)
                {
                    break;
                }
                else
                {
                    _index++;
                }

                break;

            case PaginationBehaviour.WrapAround:
                if (_index == _pages.Count - 1)
                {
                    _index = 0;
                }
                else
                {
                    _index++;
                }

                break;
        }
    }

    public async Task PreviousPageAsync()
    {
        await Task.Yield();

        switch (_behaviour)
        {
            case PaginationBehaviour.Ignore:
                if (_index == 0)
                {
                    break;
                }
                else
                {
                    _index--;
                }

                break;

            case PaginationBehaviour.WrapAround:
                if (_index == 0)
                {
                    _index = _pages.Count - 1;
                }
                else
                {
                    _index--;
                }

                break;
        }
    }

    public async Task<PaginationEmojis> GetEmojisAsync()
    {
        await Task.Yield();

        return _emojis;
    }

    public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
        => throw new NotSupportedException("This request does not support buttons.");

    public async Task<DiscordMessage> GetMessageAsync()
    {
        await Task.Yield();

        return _message;
    }

    public async Task<DiscordUser> GetUserAsync()
    {
        await Task.Yield();

        return _user;
    }

    public async Task DoCleanupAsync()
    {
        switch (PaginationDeletion)
        {
            case PaginationDeletion.DeleteEmojis:
                await _message.DeleteAllReactionsAsync();
                break;

            case PaginationDeletion.DeleteMessage:
                await _message.DeleteAsync();
                break;

            case PaginationDeletion.KeepEmojis:
                break;
        }
    }

    public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
    {
        await Task.Yield();

        return _tcs;
    }

    /// <summary>
    /// Disposes this PaginationRequest.
    /// </summary>
    public void Dispose()
    {
        // Why doesn't this class implement IDisposable?

        _ct?.Dispose();
        _tcs = null!;
    }
}
