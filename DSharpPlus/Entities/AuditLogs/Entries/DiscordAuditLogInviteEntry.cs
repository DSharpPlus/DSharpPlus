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

public sealed class DiscordAuditLogInviteEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected invite.
    /// </summary>
    public DiscordInvite Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of invite's max age change.
    /// </summary>
    public PropertyChange<int?> MaxAgeChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's code change.
    /// </summary>
    public PropertyChange<string> CodeChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's temporariness change.
    /// </summary>
    public PropertyChange<bool?> TemporaryChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's inviting member change.
    /// </summary>
    public PropertyChange<DiscordMember> InviterChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's target channel change.
    /// </summary>
    public PropertyChange<DiscordChannel> ChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's use count change.
    /// </summary>
    public PropertyChange<int?> UsesChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite's max use count change.
    /// </summary>
    public PropertyChange<int?> MaxUsesChange { get; internal set; }

    internal DiscordAuditLogInviteEntry() { }
}
