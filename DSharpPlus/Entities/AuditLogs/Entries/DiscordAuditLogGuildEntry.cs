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

public sealed class DiscordAuditLogGuildEntry : DiscordAuditLogEntry
{
    /// <summary>
    /// Gets the affected guild.
    /// </summary>
    public DiscordGuild Target { get; internal set; } = default!;

    /// <summary>
    /// Gets the description of guild name's change.
    /// </summary>
    public PropertyChange<string?> NameChange { get; internal set; }

    /// <summary>
    /// Gets the description of owner's change.
    /// </summary>
    public PropertyChange<DiscordMember?> OwnerChange { get; internal set; }

    /// <summary>
    /// Gets the description of icon's change.
    /// </summary>
    public PropertyChange<string?> IconChange { get; internal set; }

    /// <summary>
    /// Gets the description of verification level's change.
    /// </summary>
    public PropertyChange<VerificationLevel?> VerificationLevelChange { get; internal set; }

    /// <summary>
    /// Gets the description of afk channel's change.
    /// </summary>
    public PropertyChange<DiscordChannel?> AfkChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of widget channel's change.
    /// </summary>
    public PropertyChange<DiscordChannel?> EmbedChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of notification settings' change.
    /// </summary>
    public PropertyChange<DefaultMessageNotifications?> NotificationSettingsChange { get; internal set; }

    /// <summary>
    /// Gets the description of system message channel's change.
    /// </summary>
    public PropertyChange<DiscordChannel?> SystemChannelChange { get; internal set; }

    /// <summary>
    /// Gets the description of explicit content filter settings' change.
    /// </summary>
    public PropertyChange<ExplicitContentFilter?> ExplicitContentFilterChange { get; internal set; }

    /// <summary>
    /// Gets the description of guild's mfa level change.
    /// </summary>
    public PropertyChange<MfaLevel?> MfaLevelChange { get; internal set; }

    /// <summary>
    /// Gets the description of invite splash's change.
    /// </summary>
    public PropertyChange<string?> SplashChange { get; internal set; }

    /// <summary>
    /// Gets the description of the guild's region change.
    /// </summary>
    public PropertyChange<string?> RegionChange { get; internal set; }

    internal DiscordAuditLogGuildEntry() { }
}
