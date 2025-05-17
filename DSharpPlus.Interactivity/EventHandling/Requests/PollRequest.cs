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
    internal TaskCompletionSource<bool> tcs;
    internal CancellationTokenSource ct;
    internal TimeSpan timeout;
    internal ConcurrentHashSet<PollEmoji> collected;
    internal DiscordMessage message;
    internal List<DiscordEmoji> emojis;

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    /// <param name="timeout"></param>
    /// <param name="emojis"></param>
    public PollRequest(DiscordMessage message, TimeSpan timeout, IEnumerable<DiscordEmoji> emojis)
    {
        this.tcs = new TaskCompletionSource<bool>();
        this.ct = new CancellationTokenSource(timeout);
        this.ct.Token.Register(() => this.tcs.TrySetResult(true));
        this.timeout = timeout;
        this.emojis = [..emojis];
        this.collected = [];
        this.message = message;

        foreach (DiscordEmoji e in this.emojis)
        {
            this.collected.Add(new PollEmoji(e));
        }
    }

    internal void ClearCollected()
    {
        this.collected.Clear();
        foreach (DiscordEmoji e in this.emojis)
        {
            this.collected.Add(new PollEmoji(e));
        }
    }

    internal void RemoveReaction(DiscordEmoji emoji, DiscordUser member)
    {
        if (this.collected.Any(x => x.Emoji == emoji))
        {
            if (this.collected.Any(x => x.Voted.Contains(member)))
            {
                PollEmoji e = this.collected.First(x => x.Emoji == emoji);
                this.collected.TryRemove(e);
                e.Voted.TryRemove(member);
                this.collected.Add(e);
            }
        }
    }

    internal void AddReaction(DiscordEmoji emoji, DiscordUser member)
    {
        if (this.collected.Any(x => x.Emoji == emoji))
        {
            if (!this.collected.Any(x => x.Voted.Contains(member)))
            {
                PollEmoji e = this.collected.First(x => x.Emoji == emoji);
                this.collected.TryRemove(e);
                e.Voted.Add(member);
                this.collected.Add(e);
            }
        }
    }

    /// <summary>
    /// Disposes this PollRequest.
    /// </summary>
    public void Dispose()
    {
        // Why doesn't this class implement IDisposable?

        this.ct?.Dispose();
        this.tcs = null!;
    }
}

public class PollEmoji
{
    internal PollEmoji(DiscordEmoji emoji)
    {
        this.Emoji = emoji;
        this.Voted = [];
    }

    public DiscordEmoji Emoji;
    public ConcurrentHashSet<DiscordUser> Voted;
    public int Total => this.Voted.Count;
}
