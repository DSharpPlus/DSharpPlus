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
    /// Represents arguments for <see cref="DiscordClient.ThreadMembersUpdated"/> event.
    /// </summary>
    public class ThreadMembersUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the approximate number of members in the thread, capped at 50.
        /// </summary>
        public int MemberCount { get; internal set; }

        /// <summary>
        /// Gets the id of the users who were removed from the thread.
        /// </summary>
        public IReadOnlyList<ulong?> RemovedMemberIds { get; internal set; }

        /// <summary>
        /// Gets the users who were added to the thread.
        /// </summary>
        public IReadOnlyList<DiscordThreadChannelMember> AddedMembers { get; internal set; }

        /// <summary>
        /// Gets the id of the thread.
        /// </summary>
        public ulong ThreadId { get; internal set; }

        /// <summary>
        /// Gets the guild.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadMembersUpdateEventArgs"/> class.
        /// </summary>
        internal ThreadMembersUpdateEventArgs() : base() { }
    }
}
