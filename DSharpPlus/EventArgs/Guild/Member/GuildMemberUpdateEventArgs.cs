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
using System.Linq;
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
        /// Get the member with post-update info
        /// </summary>
        public DiscordMember MemberAfter { get; internal set; }

        /// <summary>
        /// Get the member with pre-update info
        /// </summary>
        public DiscordMember MemberBefore { get; internal set; }

        /// <summary>
        /// Gets a collection containing post-update roles.
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesAfter => this.MemberAfter.Roles.ToList();

        /// <summary>
        /// Gets a collection containing pre-update roles.
        /// </summary>
        public IReadOnlyList<DiscordRole> RolesBefore => this.MemberBefore.Roles.ToList();

        /// <summary>
        /// Gets the member's new nickname.
        /// </summary>
        public string NicknameAfter => this.MemberAfter.Nickname;

        /// <summary>
        /// Gets the member's old nickname.
        /// </summary>
        public string NicknameBefore => this.MemberBefore.Nickname;

        /// <summary>
        /// Gets the member's old guild avatar hash.
        /// </summary>
        public string GuildAvatarHashBefore => this.MemberBefore.GuildAvatarHash;

        /// <summary>
        /// Gets the member's new guild avatar hash.
        /// </summary>
        public string GuildAvatarHashAfter => this.MemberAfter.GuildAvatarHash;

        /// <summary>
        /// Gets the member's old username.
        /// </summary>
        public string UsernameBefore => this.MemberBefore.Username;

        /// <summary>
        /// Gets the member's new username.
        /// </summary>
        public string UsernameAfter => this.MemberAfter.Username;

        /// <summary>
        /// Gets the member's old avatar hash.
        /// </summary>
        public string AvatarHashBefore => this.MemberBefore.AvatarHash;

        /// <summary>
        /// Gets the member's new avatar hash.
        /// </summary>
        public string AvatarHashAfter => this.MemberAfter.AvatarHash;

        /// <summary>
        /// Gets whether the member had passed membership screening before the update
        /// </summary>
        public bool? PendingBefore => this.MemberBefore.IsPending;

        /// <summary>
        /// Gets whether the member had passed membership screening after the update
        /// </summary>
        public bool? PendingAfter => this.MemberAfter.IsPending;

        /// <summary>
        /// Gets the member's communication restriction before the update
        /// </summary>
        public DateTimeOffset? CommunicationDisabledUntilBefore => this.MemberBefore.CommunicationDisabledUntil;

        /// <summary>
        /// Gets the member's communication restriction after the update
        /// </summary>
        public DateTimeOffset? CommunicationDisabledUntilAfter => this.MemberAfter.CommunicationDisabledUntil;

        /// <summary>
        /// Gets the member that was updated.
        /// </summary>
        public DiscordMember Member => this.MemberAfter;

        internal GuildMemberUpdateEventArgs() : base() { }
    }
}
