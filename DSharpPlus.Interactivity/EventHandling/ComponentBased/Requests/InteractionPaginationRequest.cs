// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;

namespace DSharpPlus.Interactivity.EventHandling
{
    internal class InteractionPaginationRequest : IPaginationRequest
    {
        private int _index;
        private readonly List<Page> _pages = new();

        private readonly TaskCompletionSource<bool> _tcs = new();


        private DiscordInteraction _lastInteraction;
        private CancellationTokenSource _interactionCts;

        private readonly CancellationToken _token;
        private readonly DiscordUser _user;
        private readonly DiscordMessage _message;
        private readonly PaginationButtons _buttons;
        private readonly PaginationBehaviour _wrapBehavior;
        private readonly ButtonPaginationBehavior _behaviorBehavior;


        public InteractionPaginationRequest(DiscordInteraction interaction, DiscordMessage message, DiscordUser user,
            PaginationBehaviour behavior, ButtonPaginationBehavior behaviorBehavior,
            PaginationButtons buttons, IEnumerable<Page> pages, CancellationToken token)
        {
            this._user = user;
            this._token = token;
            this._buttons = new(buttons);
            this._message = message;
            this._wrapBehavior = behavior;
            this._behaviorBehavior = behaviorBehavior;
            this._pages.AddRange(pages);

            this.RegenerateCTS(interaction);
            this._token.Register(() => this._tcs.TrySetResult(false));
        }

        public int PageCount => this._pages.Count;

        internal void RegenerateCTS(DiscordInteraction interaction)
        {
            this._interactionCts?.Dispose();
            this._lastInteraction = interaction;
            this._interactionCts = new(TimeSpan.FromSeconds((60 * 15) - 5));
            this._interactionCts.Token.Register(() => this._tcs.TrySetResult(false));
        }

        public Task<Page> GetPageAsync()
        {
            var page = Task.FromResult(this._pages[this._index]);

            if (this.PageCount is 1)
            {
                this._buttons.ButtonArray.Select(b => b.Disable());
                this._buttons.Stop.Enable();
                return page;
            }

            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
                return page;

            this._buttons.SkipLeft.Disabled = this._index < 2;

            this._buttons.Left.Disabled = this._index < 1;

            this._buttons.Right.Disabled = this._index == this.PageCount - 1;

            this._buttons.SkipRight.Disabled = this._index >= this.PageCount - 2;

            return page;
        }

        public Task SkipLeftAsync()
        {
            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                this._index = this._index is 0 ? this._pages.Count - 1 : 0;
                return Task.CompletedTask;
            }

            this._index = 0;

            return Task.CompletedTask;
        }

        public Task SkipRightAsync()
        {
            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                this._index = this._index == this.PageCount - 1 ? 0 : this.PageCount - 1;
                return Task.CompletedTask;
            }

            this._index = this._pages.Count - 1;

            return Task.CompletedTask;
        }

        public Task NextPageAsync()
        {
            this._index++;

            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                if (this._index >= this.PageCount)
                    this._index = 0;

                return Task.CompletedTask;
            }

            this._index = Math.Min(this._index, this.PageCount - 1);

            return Task.CompletedTask;
        }

        public Task PreviousPageAsync()
        {
            this._index--;

            if (this._wrapBehavior is PaginationBehaviour.WrapAround)
            {
                if (this._index is - 1)
                    this._index = this._pages.Count - 1;

                return Task.CompletedTask;
            }

            this._index = Math.Max(this._index, 0);

            return Task.CompletedTask;
        }

        public Task<PaginationEmojis> GetEmojisAsync()
            => Task.FromException<PaginationEmojis>(new NotSupportedException("Emojis aren't supported for this request."));

        public Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync()
            => Task.FromResult((IEnumerable<DiscordButtonComponent>)this._buttons.ButtonArray);

        public Task<DiscordMessage> GetMessageAsync() => Task.FromResult(this._message);

        public Task<DiscordUser> GetUserAsync() => Task.FromResult(this._user);

        public Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync() => Task.FromResult(this._tcs);

        // This is essentially the stop method. //
        public async Task DoCleanupAsync()
        {
            switch (this._behaviorBehavior)
            {
                case ButtonPaginationBehavior.Disable:
                    var buttons = this._buttons.ButtonArray
                        .Select(b => new DiscordButtonComponent(b))
                        .Select(b => b.Disable());

                    var builder = new DiscordWebhookBuilder()
                        .WithContent(this._pages[this._index].Content)
                        .AddEmbed(this._pages[this._index].Embed)
                        .AddComponents(buttons);

                    await this._lastInteraction.EditOriginalResponseAsync(builder);
                    break;

                case ButtonPaginationBehavior.DeleteButtons:
                    builder = new DiscordWebhookBuilder()
                        .WithContent(this._pages[this._index].Content)
                        .AddEmbed(this._pages[this._index].Embed);

                    await this._lastInteraction.EditOriginalResponseAsync(builder);
                    break;

                case ButtonPaginationBehavior.DeleteMessage:
                    await this._lastInteraction.DeleteOriginalResponseAsync();
                    break;

                case ButtonPaginationBehavior.Ignore:
                    break;
            }
        }
    }
}
