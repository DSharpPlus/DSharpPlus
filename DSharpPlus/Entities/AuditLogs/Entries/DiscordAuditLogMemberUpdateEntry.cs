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

namespace DSharpPlus.Entities.AuditLogs;
public sealed class DiscordAuditLogMemberUpdateEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected member.
    /// </summary>
    public DiscordMember Target { get; internal set; }

    /// <summary>
    /// Gets the description of member's nickname change.
    /// </summary>
    public PropertyChange<string> NicknameChange { get; internal set; }

    /// <summary>
    /// Gets the roles that were removed from the member.
    /// </summary>
    public IReadOnlyList<DiscordRole> RemovedRoles { get; internal set; }

    /// <summary>
    /// Gets the roles that were added to the member.
    /// </summary>
    public IReadOnlyList<DiscordRole> AddedRoles { get; internal set; }

    /// <summary>
    /// Gets the description of member's mute status change.
    /// </summary>
    public PropertyChange<bool?> MuteChange { get; internal set; }

    /// <summary>
    /// Gets the description of member's deaf status change.
    /// </summary>
    public PropertyChange<bool?> DeafenChange { get; internal set; }

    /// <summary>
    /// Gets the change in a user's timeout status
    /// </summary>
    public PropertyChange<DateTime?> TimeoutChange { get; internal set; }

    internal DiscordAuditLogMemberUpdateEntry() { }
}
