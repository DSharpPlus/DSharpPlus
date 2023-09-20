using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.EventHandling
{
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
            this._tcs = new TaskCompletionSource<bool>();
            this._ct = new CancellationTokenSource(timeout);
            this._ct.Token.Register(() => this._tcs.TrySetResult(true));
            this._timeout = timeout;
            this._emojis = emojis;
            this._collected = new ConcurrentHashSet<PollEmoji>();
            this._message = message;

            foreach (var e in emojis)
            {
                this._collected.Add(new PollEmoji(e));
            }
        }

        internal void ClearCollected()
        {
            this._collected.Clear();
            foreach (var e in this._emojis)
            {
                this._collected.Add(new PollEmoji(e));
            }
        }

        internal void RemoveReaction(DiscordEmoji emoji, DiscordUser member)
        {
            if (this._collected.Any(x => x.Emoji == emoji))
            {
                if (this._collected.Any(x => x.Voted.Contains(member)))
                {
                    var e = this._collected.First(x => x.Emoji == emoji);
                    this._collected.TryRemove(e);
                    e.Voted.TryRemove(member);
                    this._collected.Add(e);
                }
            }
        }

        internal void AddReaction(DiscordEmoji emoji, DiscordUser member)
        {
            if (this._collected.Any(x => x.Emoji == emoji))
            {
                if (!this._collected.Any(x => x.Voted.Contains(member)))
                {
                    var e = this._collected.First(x => x.Emoji == emoji);
                    this._collected.TryRemove(e);
                    e.Voted.Add(member);
                    this._collected.Add(e);
                }
            }
        }

        /// <summary>
        /// Disposes this PollRequest.
        /// </summary>
        public void Dispose()
        {
            // Why doesn't this class implement IDisposable?

            this._ct?.Dispose();
            this._tcs = null!;
        }
    }

    public class PollEmoji
    {
        internal PollEmoji(DiscordEmoji emoji)
        {
            this.Emoji = emoji;
            this.Voted = new ConcurrentHashSet<DiscordUser>();
        }

        public DiscordEmoji Emoji;
        public ConcurrentHashSet<DiscordUser> Voted;
        public int Total => this.Voted.Count;
    }
}
