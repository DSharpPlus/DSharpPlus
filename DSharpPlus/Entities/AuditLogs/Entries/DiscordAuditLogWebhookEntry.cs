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

namespace DSharpPlus.Entities.AuditLogs;

public sealed class DiscordAuditLogWebhookEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected webhook.
    /// </summary>
    public DiscordWebhook Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of webhook's name change.
    /// </summary>
    public PropertyChange<string> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of webhook's target channel change.
    /// </summary>
    public PropertyChange<DiscordChannel> ChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of webhook's type change.
    /// </summary>
    public PropertyChange<int?> TypeChange { get; internal set; }

    /// <summary>
    /// Gets the description of webhook's avatar change.
    /// </summary>
    public PropertyChange<string> AvatarHashChange { get; internal set; }

    /// <summary>
    /// Gets the change in application ID.
    /// </summary>
    public PropertyChange<ulong?> ApplicationIdChange { get; internal set; }

    internal DiscordAuditLogWebhookEntry() { }
}
