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

using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.ThreadListSynced"/> event.
    /// </summary>
    public class ThreadListSyncEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets all thread member objects, indicating which threads the current user has been added to.
        /// </summary>
        public IReadOnlyList<DiscordThreadChannelMember> CurrentMembers { get; internal set; }

        /// <summary>
        /// Gets all active threads in the given channels that the current user can access.
        /// </summary>
        public IReadOnlyList<DiscordThreadChannel> Threads { get; internal set; }

        /// <summary>
        /// Gets the parent channels whose threads are being synced. May contain channels that have no active threads as well.
        /// </summary>
        public IReadOnlyList<DiscordChannel> Channels { get; internal set; }

        /// <summary>
        /// Gets the guild being synced.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        internal ThreadListSyncEventArgs() : base() { }
    }
}
