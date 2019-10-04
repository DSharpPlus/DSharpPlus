using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
    public class PollRequest
    {
        internal TaskCompletionSource<bool> _tcs;
        internal CancellationTokenSource _ct;
        internal TimeSpan _timeout;
        internal ConcurrentHashSet<PollEmoji> _collected;
        internal DiscordMessage _message;
        internal DiscordEmoji[] _emojis;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timeout"></param>
        /// <param name="emojis"></param>
        public PollRequest(DiscordMessage message, TimeSpan timeout, params DiscordEmoji[] emojis)
        {
            this._tcs = new TaskCompletionSource<bool>();
            this._ct = new CancellationTokenSource(timeout);
            this._ct.Token.Register(() => _tcs.TrySetResult(true));
            this._timeout = timeout;
            this._emojis = emojis;
            this._collected = new ConcurrentHashSet<PollEmoji>();
            this._message = message;
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
                    var e = _collected.First(x => x.Emoji == emoji);
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
                    var e = _collected.First(x => x.Emoji == emoji);
                    _collected.TryRemove(e);
                    e.Voted.Add(member);
                    _collected.Add(e);
                }
            }
        }

        ~PollRequest()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this PollRequest.
        /// </summary>
        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null;
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
        public int Total => Voted.Count;
    }
}
