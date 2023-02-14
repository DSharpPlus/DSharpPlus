// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023

 DSharpPlus Contributors
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

using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity.EventHandling
{
    public interface IPaginationRequest
    {
        /// <summary>
        /// Returns the number of pages.
        /// </summary>
        /// <returns></returns>
        int PageCount { get; }

        /// <summary>
        /// Returns the current page.
        /// </summary>
        /// <returns></returns>
        Task<Page> GetPageAsync();

        /// <summary>
        /// Tells the request to set its index to the first page.
        /// </summary>
        /// <returns></returns>
        Task SkipLeftAsync();

        /// <summary>
        /// Tells the request to set its index to the last page.
        /// </summary>
        /// <returns></returns>
        Task SkipRightAsync();

        /// <summary>
        /// Tells the request to increase its index by one.
        /// </summary>
        /// <returns></returns>
        Task NextPageAsync();

        /// <summary>
        /// Tells the request to decrease its index by one.
        /// </summary>
        /// <returns></returns>
        Task PreviousPageAsync();

        /// <summary>
        /// Requests message emojis from pagination request.
        /// </summary>
        /// <returns></returns>
        Task<PaginationEmojis> GetEmojisAsync();

        /// <summary>
        /// Requests the message buttons from the pagination request.
        /// </summary>
        /// <returns>The buttons.</returns>
        Task<IEnumerable<DiscordButtonComponent>> GetButtonsAsync();

        /// <summary>
        /// Gets pagination message from this request.
        /// </summary>
        /// <returns></returns>
        Task<DiscordMessage> GetMessageAsync();

        /// <summary>
        /// Gets the user this pagination applies to.
        /// </summary>
        /// <returns></returns>
        Task<DiscordUser> GetUserAsync();

        /// <summary>
        /// Get this request's Task Completion Source.
        /// </summary>
        /// <returns></returns>
        Task<TaskCompletionSource<bool>> GetTaskCompletionSourceAsync();

        /// <summary>
        /// Tells the request to perform cleanup.
        /// </summary>
        /// <returns></returns>
        Task DoCleanupAsync();
    }
}
