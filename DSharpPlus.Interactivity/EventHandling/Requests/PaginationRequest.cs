using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Concurrency;
using DSharpPlus.Interactivity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Interactivity.EventHandling
{
    public class PaginationRequest
    {
        internal TaskCompletionSource<bool> _tcs;
        internal CancellationTokenSource _ct;
        internal TimeSpan _timeout;
        internal List<Page> _pages;
        internal PaginationBehaviour _behaviour;
        internal PaginationDeletion _deletion;
        internal DiscordMessage _message;
        internal PaginationEmojis _emojis;
        internal DiscordUser _user;
        int index = 0;

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
        public PaginationRequest(DiscordMessage message, DiscordUser user, PaginationBehaviour behaviour, PaginationDeletion deletion,
            PaginationEmojis emojis, TimeSpan timeout, params Page[] pages)
        {
            this._tcs = new TaskCompletionSource<bool>();
            this._ct = new CancellationTokenSource(timeout);
            this._ct.Token.Register(() => _tcs.TrySetResult(true));
            this._timeout = timeout;

            this._message = message;
            this._user = user;

            this._deletion = deletion;
            this._behaviour = behaviour;
            this._emojis = emojis;

            this._pages = new List<Page>();
            foreach(var p in pages)
            {
                this._pages.Add(p);
            }
        }

        internal Page GetPage()
        {
            return _pages[index];
        }

        internal void SkipLeft()
        {
            index = 0;
        }

        internal void SkipRight()
        {
            index = _pages.Count - 1;
        }

        internal void NextPage()
        {
            switch (_behaviour)
            {
                case PaginationBehaviour.Default:
                case PaginationBehaviour.Ignore:
                    if(index == _pages.Count - 1)
                        break;
                    else
                        index++;

                    break;

                case PaginationBehaviour.WrapAround:
                    if (index == _pages.Count - 1)
                        index = 0;
                    else
                        index++;

                    break;
            }
        }

        internal void PreviousPage()
        {
            switch (_behaviour)
            {
                case PaginationBehaviour.Default:
                case PaginationBehaviour.Ignore:
                    if (index == 0)
                        break;
                    else
                        index--;

                    break;

                case PaginationBehaviour.WrapAround:
                    if (index == 0)
                        index = _pages.Count - 1;
                    else
                        index--;

                    break;
            }
        }

        ~PaginationRequest()
        {
            this.Dispose();
        }

        /// <summary>
        /// Disposes this PaginationRequest.
        /// </summary>
        public void Dispose()
        {
            this._ct.Dispose();
            this._tcs = null;
        }
    }

    public class PaginationEmojis
    {
        public DiscordEmoji SkipLeft;
        public DiscordEmoji SkipRight;
        public DiscordEmoji Left;
        public DiscordEmoji Right;
        public DiscordEmoji Stop;

        public PaginationEmojis(DiscordClient client)
        {
            Left = DiscordEmoji.FromUnicode(client, "◀");
            Right = DiscordEmoji.FromUnicode(client, "▶");
            SkipLeft = DiscordEmoji.FromUnicode(client, "⏮");
            SkipRight = DiscordEmoji.FromUnicode(client, "⏭");
            Stop = DiscordEmoji.FromUnicode(client, "⏹");
        }
    }

    public class Page
    {
        public string Content { get; private set; }
        public DiscordEmbed Embed { get; private set; }

        public Page(string content, DiscordEmbedBuilder embed)
        {
            this.Content = content;
            this.Embed = embed?.Build();
        }
    }
}
