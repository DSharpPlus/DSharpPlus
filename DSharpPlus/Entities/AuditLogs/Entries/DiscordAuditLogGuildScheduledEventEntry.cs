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

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogGuildScheduledEventEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets a change in the event's name
    /// </summary>
    public PropertyChange<string?> Name { get; internal set; }

    /// <summary>
    /// Gets the target event. Note that this will only have the ID specified if it is not cached.
    /// </summary>
    public DiscordScheduledGuildEvent Target { get; internal set; }

    /// <summary>
    /// Gets the channel the event was changed to.
    /// </summary>
    public PropertyChange<DiscordChannel?> Channel { get; internal set; }

    /// <summary>
    /// Gets the description change of the event.
    /// </summary>
    public PropertyChange<string?> Description { get; internal set; }

    /// <summary>
    /// Gets the change of type for the event.
    /// </summary>
    public PropertyChange<ScheduledGuildEventType?> Type { get; internal set; }

    /// <summary>
    /// Gets the change in image hash.
    /// </summary>
    public PropertyChange<string?> ImageHash { get; internal set; }

    /// <summary>
    /// Gets the change in event location, if it's an external event.
    /// </summary>
    public PropertyChange<string?> Location { get; internal set; }

    /// <summary>
    /// Gets change in privacy level.
    /// </summary>
    public PropertyChange<ScheduledGuildEventPrivacyLevel?> PrivacyLevel { get; internal set; }

    /// <summary>
    /// Gets the change in status.
    /// </summary>
    public PropertyChange<ScheduledGuildEventStatus?> Status { get; internal set; }

    public DiscordAuditLogGuildScheduledEventEntry() { }
}
