// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
        private TimeSpan _timeout;
        private readonly List<Page> _pages;
        private readonly PaginationBehaviour _behaviour;
        private readonly DiscordMessage _message;
        private readonly PaginationEmojis _emojis;
        private readonly DiscordUser _user;
        private int index = 0;

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
            foreach (var p in pages)
            {
                this._pages.Add(p);
            }
        }

        public int PageCount => this._pages.Count;

        public PaginationDeletion PaginationDeletion { get; }

        public async Task<Page> GetPageAsync()
        {
            await Task.Yield();

            return this._pages[this.index];
        }

        public async Task SkipLeftAsync()
        {
            await Task.Yield();

            this.index = 0;
        }

        public async Task SkipRightAsync()
        {
            await Task.Yield();

            this.index = this._pages.Count - 1;
        }

        public async Task NextPageAsync()
        {
            await Task.Yield();

            switch (this._behaviour)
            {
                case PaginationBehaviour.Ignore:
                    if (this.index == this._pages.Count - 1)
                        break;
                    else
                        this.index++;

                    break;

                case PaginationBehaviour.WrapAround:
                    if (this.index == this._pages.Count - 1)
                        this.index = 0;
                    else
                        this.index++;

                    break;
            }
        }

        public async Task PreviousPageAsync()
        {
            await Task.Yield();

            switch (this._behaviour)
            {
                case PaginationBehaviour.Ignore:
                    if (this.index == 0)
                        break;
                    else
                        this.index--;

                    break;

                case PaginationBehaviour.WrapAround:
                    if (this.index == 0)
                        this.index = this._pages.Count - 1;
                    else
                        this.index--;

                    break;
            }
        }

        public async Task<PaginationEmojis> GetEmojisAsync()
        {
            await Task.Yield();

            return this._emojis;
        }

        public async Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
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
                    await this._message.DeleteAllReactionsAsync().ConfigureAwait(false);
                    break;

                case PaginationDeletion.DeleteMessage:
                    await this._message.DeleteAsync().ConfigureAwait(false);
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
