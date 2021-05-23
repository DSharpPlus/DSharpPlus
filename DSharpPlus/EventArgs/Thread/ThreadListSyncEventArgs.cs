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
        /// Gets the id of the guild.
        /// </summary>
        /// 
        public ulong GuildId { get; internal set; }

        /// <summary>
        /// Gets the parent channel ids whose threads are being synced. If omitted, then threads were synced for the entire guild. This array may contain channel_ids that have no active threads as well.
        /// </summary>
        /// 
        public IEnumerable<ulong?> ChannelIds { get; internal set; }

        /// <summary>
        /// Gets all active threads in the given channels that the current user can access.
        /// </summary>
        /// 
        public IEnumerable<DiscordThreadChannel> Threads { get; internal set; }

        /// <summary>
        /// Gets all thread member objects from the synced threads for the current user, indicating which threads the current user has been added to
        /// </summary>
        /// 
        public IEnumerable<DiscordThreadChannelMember> Members { get; internal set; }

        internal ThreadListSyncEventArgs() : base() { }
    }
}
