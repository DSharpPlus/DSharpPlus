// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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
using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.GuildMemberUpdated"/> event.
    /// </summary>
    public class GuildMemberUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the guild in which the update occurred.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }

        /// <summary>
        /// Gets a collection containing post-update roles.
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesAfter { get; internal set; }

        /// <summary>
        /// Gets a collection containing pre-update roles.
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesBefore { get; internal set; }

        /// <summary>
        /// Gets the member's new nickname.
        /// </summary>
        public string NicknameAfter { get; internal set; }

        /// <summary>
        /// Gets the member's old nickname.
        /// </summary>
        public string NicknameBefore { get; internal set; }

        /// <summary>
        /// Gets the member's old avatar hash.
        /// </summary>
        public string AvatarHashBefore { get; internal set; }

        /// <summary>
        /// Gets the member's new avatar hash.
        /// </summary>
        public string AvatarHashAfter { get; internal set; }

        /// <summary>
        /// Gets whether the member had passed membership screening before the update
        /// </summary>
        public bool? PendingBefore { get; internal set; }

        /// <summary>
        /// Gets whether the member had passed membership screening after the update
        /// </summary>
        public bool? PendingAfter { get; internal set; }

        /// <summary>
        /// Gets the member's communication restriction before the update
        /// </summary>
        public DateTimeOffset? CommunicationDisabledUntilBefore { get; internal set; }

        /// <summary>
        /// Gets the member's communication restriction after the update
        /// </summary>
        public DateTimeOffset? CommunicationDisabledUntilAfter { get; internal set; }

        /// <summary>
        /// Gets the member that was updated.
        /// </summary>
        public DiscordMember Member { get; internal set; }

        internal GuildMemberUpdateEventArgs() : base() { }
    }
}
