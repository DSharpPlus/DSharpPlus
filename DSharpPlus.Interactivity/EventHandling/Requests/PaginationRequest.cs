using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;

namespace DSharpPlus.Interactivity.EventHandling
{
    internal class PaginationRequest : IPaginationRequest
    {
        private TaskCompletionSource<bool> _tcs;
        private readonly CancellationTokenSource _ct;
        private readonly TimeSpan _timeout;
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
            this._tcs = new();
            this._ct = new(timeout);
            this._ct.Token.Register(() => this._tcs.TrySetResult(true));
            this._timeout = timeout;

            this._message = message;
            this._user = user;

            this.PaginationDeletion = deletion;
            this._behaviour = behaviour;
            this._emojis = emojis;

            this._pages = new List<Page>();
            foreach (Page p in pages)
            {
                this._pages.Add(p);
            }
        }

        public int PageCount => this._pages.Count;

        public PaginationDeletion PaginationDeletion { get; }

        public async Task<Page> GetPageAsync()
        {
            await Task.Yield();

            return this._pages[this._index];
        }

        public async Task SkipLeftAsync()
        {
            await Task.Yield();

            this._index = 0;
        }

        public async Task SkipRightAsync()
        {
            await Task.Yield();

            this._index = this._pages.Count - 1;
        }

        public async Task NextPageAsync()
        {
            await Task.Yield();

            switch (this._behaviour)
            {
                case PaginationBehaviour.Ignore:
                    if (this._index == this._pages.Count - 1)
                    {
                        break;
                    }
                    else
                    {
                        this._index++;
                    }

                    break;

                case PaginationBehaviour.WrapAround:
                    if (this._index == this._pages.Count - 1)
                    {
                        this._index = 0;
                    }
                    else
                    {
                        this._index++;
                    }

                    break;
            }
        }

        public async Task PreviousPageAsync()
        {
            await Task.Yield();

            switch (this._behaviour)
            {
                case PaginationBehaviour.Ignore:
                    if (this._index == 0)
                    {
                        break;
                    }
                    else
                    {
                        this._index--;
                    }

                    break;

                case PaginationBehaviour.WrapAround:
                    if (this._index == 0)
                    {
                        this._index = this._pages.Count - 1;
                    }
                    else
                    {
                        this._index--;
                    }

                    break;
            }
        }

        public async Task<PaginationEmojis> GetEmojisAsync()
        {
            await Task.Yield();

            return this._emojis;
        }

        public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
            => throw new NotSupportedException("This request does not support buttons.");

        public async Task<DiscordMessage> GetMessageAsync()
        {
            await Task.Yield();

            return this._message;
        }

        public async Task<DiscordUser> GetUserAsync()
        {
            await Task.Yield();

            return this._user;
        }

        public async Task DoCleanupAsync()
        {
            switch (this.PaginationDeletion)
            {
                case PaginationDeletion.DeleteEmojis:
                    await this._message.DeleteAllReactionsAsync();
                    break;

                case PaginationDeletion.DeleteMessage:
                    await this._message.DeleteAsync();
                    break;

                case PaginationDeletion.KeepEmojis:
                    break;
            }
        }

        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
        {
            await Task.Yield();

            return this._tcs;
        }

        /// <summary>
        /// Disposes this PaginationRequest.
        /// </summary>
        public void Dispose()
        {
            // Why doesn't this class implement IDisposable?

            this._ct?.Dispose();
            this._tcs = null!;
        }
    }
}

namespace DSharpPlus.Interactivity
{
    public class PaginationEmojis
    {
        public DiscordEmoji SkipLeft;
        public DiscordEmoji SkipRight;
        public DiscordEmoji Left;
        public DiscordEmoji Right;
        public DiscordEmoji Stop;

        public PaginationEmojis()
        {
            this.Left = DiscordEmoji.FromUnicode("◀");
            this.Right = DiscordEmoji.FromUnicode("▶");
            this.SkipLeft = DiscordEmoji.FromUnicode("⏮");
            this.SkipRight = DiscordEmoji.FromUnicode("⏭");
            this.Stop = DiscordEmoji.FromUnicode("⏹");
        }
    }

    public class Page
    {
        public string Content { get; set; }
        public DiscordEmbed Embed { get; set; }

        public Page(string content = "", DiscordEmbedBuilder embed = null)
        {
            this.Content = content;
            this.Embed = embed?.Build();
        }
    }
}
