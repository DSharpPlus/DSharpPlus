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

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogRoleUpdateEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected role.
    /// </summary>
    public DiscordRole Target { get; internal set; }

    /// <summary>
    /// Gets the description of role's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of role's color change.
    /// </summary>
    public PropertyChange<int?> ColorChange { get; internal set; }

    /// <summary>
    /// Gets the description of role's permission set change.
    /// </summary>
    public PropertyChange<Permissions?> PermissionChange { get; internal set; }

    /// <summary>
    /// Gets the description of the role's position change.
    /// </summary>
    public PropertyChange<int?> PositionChange { get; internal set; }

    /// <summary>
    /// Gets the description of the role's mentionability change.
    /// </summary>
    public PropertyChange<bool?> MentionableChange { get; internal set; }

    /// <summary>
    /// Gets the description of the role's hoist status change.
    /// </summary>
    public PropertyChange<bool?> HoistChange { get; internal set; }

    internal DiscordAuditLogRoleUpdateEntry() { }
}
