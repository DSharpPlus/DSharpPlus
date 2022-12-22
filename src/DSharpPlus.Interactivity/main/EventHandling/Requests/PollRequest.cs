// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
        _collected = new ConcurrentHashSet<PollEmoji>();
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

    ~PollRequest()
    {
        Dispose();
    }

    /// <summary>
    /// Disposes this PollRequest.
    /// </summary>
    public void Dispose()
    {
        _ct.Dispose();
        _tcs = null;
    }
}

public class PollEmoji
{
    internal PollEmoji(DiscordEmoji emoji)
    {
        Emoji = emoji;
        Voted = new ConcurrentHashSet<DiscordUser>();
    }

    public DiscordEmoji Emoji;
    public ConcurrentHashSet<DiscordUser> Voted;
    public int Total => Voted.Count;
}
