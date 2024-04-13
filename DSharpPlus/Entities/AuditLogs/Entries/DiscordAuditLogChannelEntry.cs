// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2024 DSharpPlus Contributors
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

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogChannelEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected channel.
    /// </summary>
    public DiscordChannel Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of channel's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's type change.
    /// </summary>
    public PropertyChange<DiscordChannelType?> TypeChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's nsfw flag change.
    /// </summary>
    public PropertyChange<bool?> NsfwChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's bitrate change.
    /// </summary>
    public PropertyChange<int?> BitrateChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel permission overwrites' change.
    /// </summary>
    public PropertyChange<IReadOnlyList<DiscordOverwrite>> OverwriteChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's topic change.
    /// </summary>
    public PropertyChange<string> TopicChange { get; internal set; }

    /// <summary>
    /// Gets the description of channel's slow mode timeout change.
    /// </summary>
    public PropertyChange<int?> PerUserRateLimitChange { get; internal set; }

    public PropertyChange<int?> UserLimit { get; internal set; }

    public PropertyChange<DiscordChannelFlags?> Flags { get; internal set; }

    public PropertyChange<IEnumerable<DiscordForumTag>> AvailableTags { get; internal set; }

    internal DiscordAuditLogChannelEntry() { }
}
