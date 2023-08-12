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

namespace DSharpPlus.Entities;

public sealed class DiscordAuditLogThreadEventEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the target thread.
    /// </summary>
    public DiscordThreadChannel Target { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's name.
    /// </summary>
    public PropertyChange<string?> Name { get; internal set; }

    /// <summary>
    /// Gets a change in channel type.
    /// </summary>
    public PropertyChange<ChannelType?> Type { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's archived status.
    /// </summary>
    public PropertyChange<bool?> Archived { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's auto archive duration.
    /// </summary>
    public PropertyChange<int?> AutoArchiveDuration { get; internal set; }

    /// <summary>
    /// Gets a change in the threads invitibility
    /// </summary>
    public PropertyChange<bool?> Invitable { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's locked status
    /// </summary>
    public PropertyChange<bool?> Locked { get; internal set; }

    /// <summary>
    /// Gets a change in the thread's slowmode setting
    /// </summary>
    public PropertyChange<int?> PerUserRateLimit { get; internal set; }

    internal DiscordAuditLogThreadEventEntry() { }
}
