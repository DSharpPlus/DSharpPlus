using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus.Test
{
    /// <summary>
    /// An example implementation of the IPaginationRequest interface.
    /// Take a look at the IPaginationRequest interface in DSharpPlus.Interactivity for more information.
    /// </summary>
    public class TestBotPaginator : IPaginationRequest
    {
        private List<Page> pages;
        private TaskCompletionSource<bool> _tcs;
        private CancellationTokenSource _cts;
        private DiscordMessage _msg;
        private int index = 0;
        private PaginationEmojis _emojis;
        private DiscordUser _usr;

        public TestBotPaginator(DiscordClient client, DiscordUser usr, DiscordMessage msg, List<Page> pages)
        {
            this.pages = pages;
            this._tcs = new TaskCompletionSource<bool>();
            this._cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            this._cts.Token.Register(() => this._tcs.TrySetResult(true));
            this._msg = msg;
            this._emojis = new PaginationEmojis();
            this._usr = usr;
        }

        public async Task DoCleanupAsync()
        {
            await this._msg.DeleteAsync();
        }

        public async Task<PaginationEmojis> GetEmojisAsync()
        {
            await Task.Yield();
            return this._emojis;
        }

        public async Task<DiscordMessage> GetMessageAsync()
        {
            await Task.Yield();
            return this._msg;
        }

        public async Task<Page> GetPageAsync()
        {
            await Task.Yield();
            return this.pages[this.index];
        }

        public async Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync()
        {
            await Task.Yield();
            return this._tcs;
        }

        public async Task<DiscordUser> GetUserAsync()
        {
            await Task.Yield();
            return this._usr;
        }

        public async Task NextPageAsync()
        {
            await Task.Yield();

            if (this.index < pages.Count - 1)
                this.index++;
        }

        public async Task PreviousPageAsync()
        {
            await Task.Yield();

            if (this.index > 0)
                this.index--;
        }

        public async Task SkipLeftAsync()
        {
            await Task.Yield();

            this.index = 0;
        }

        public async Task SkipRightAsync()
        {
            await Task.Yield();

            this.index = this.pages.Count - 1;
        }
    }
}
