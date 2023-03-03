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
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Interactivity.EventHandling
{
    /// <summary>
    /// Represents a match request for a modal of the given Id and predicate.
    /// </summary>
    internal class ModalMatchRequest
    {
        /// <summary>
        /// The custom Id of the modal.
        /// </summary>
        public string ModalId { get; }

        /// <summary>
        /// The completion source that represents the result of the match.
        /// </summary>
        public TaskCompletionSource<ModalSubmitEventArgs> Tcs { get; private set; } = new();

        protected CancellationToken Cancellation { get; }

        /// <summary>
        /// The predicate/criteria that this match will be fulfilled under.
        /// </summary>
        protected Func<ModalSubmitEventArgs, bool> Predicate { get; }

        public ModalMatchRequest(string modal_id, Func<ModalSubmitEventArgs, bool> predicate, CancellationToken cancellation)
        {
            this.ModalId = modal_id;
            this.Predicate = predicate;
            this.Cancellation = cancellation;
            this.Cancellation.Register(() => this.Tcs.TrySetResult(null)); // "TrySetCancelled would probably be better but I digress" - Velvet // "TrySetCancelled throws an exception when you await the task, actually" - Velvet, 2022
        }

        /// <summary>
        /// Checks whether the <see cref="ModalSubmitEventArgs"/> matches the predicate criteria.
        /// </summary>
        /// <param name="args">The <see cref="ModalSubmitEventArgs"/> to check.</param>
        /// <returns>Whether the <see cref="ModalSubmitEventArgs"/> matches the predicate.</returns>
        public bool IsMatch(ModalSubmitEventArgs args)
            => this.Predicate(args);
    }
}
