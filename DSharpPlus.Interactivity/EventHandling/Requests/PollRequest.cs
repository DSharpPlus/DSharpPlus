using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.EventHandling;

public class PollRequest
{
    internal TaskCompletionSource<bool> _tcs;
    internal CancellationTokenSource _ct;
    internal TimeSpan _timeout;
    internal ConcurrentHashSet<PollEmoji> _collected;
    internal DiscordMessage _message;
    internal IEnumerable<DiscordEmoji> _emojis;

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <param name="timeout"></param>
    /// <param name="emojis"></param>
    public PollRequest(DiscordMessage message, TimeSpan timeout, IEnumerable<DiscordEmoji> emojis)
    {
        _tcs = new TaskCompletionSource<bool>();
        _ct = new CancellationTokenSource(timeout);
        _ct.Token.Register(() => _tcs.TrySetResult(true));
        _timeout = timeout;
        _emojis = emojis;
        _collected = [];
        _message = message;

        foreach (DiscordEmoji e in emojis)
        {
            _collected.Add(new PollEmoji(e));
        }
    }

    internal void ClearCollected()
    {
        _collected.Clear();
        foreach (DiscordEmoji e in _emojis)
        {
            _collected.Add(new PollEmoji(e));
        }
    }

    internal void RemoveReaction(DiscordEmoji emoji, DiscordUser member)
    {
        if (_collected.Any(x => x.Emoji == emoji))
        {
            if (_collected.Any(x => x.Voted.Contains(member)))
            {
                PollEmoji e = _collected.First(x => x.Emoji == emoji);
                _collected.TryRemove(e);
                e.Voted.TryRemove(member);
                _collected.Add(e);
            }
        }
    }

    internal void AddReaction(DiscordEmoji emoji, DiscordUser member)
    {
        if (_collected.Any(x => x.Emoji == emoji))
        {
            if (!_collected.Any(x => x.Voted.Contains(member)))
            {
                PollEmoji e = _collected.First(x => x.Emoji == emoji);
                _collected.TryRemove(e);
                e.Voted.Add(member);
                _collected.Add(e);
            }
        }
    }

    /// <summary>
    /// Disposes this PollRequest.
    /// </summary>
    public void Dispose()
    {
        // Why doesn't this class implement IDisposable?

        _ct?.Dispose();
        _tcs = null!;
    }
}

public class PollEmoji
{
    internal PollEmoji(DiscordEmoji emoji)
    {
        Emoji = emoji;
        Voted = [];
    }

    public DiscordEmoji Emoji;
    public ConcurrentHashSet<DiscordUser> Voted;
    public int Total => Voted.Count;
}
