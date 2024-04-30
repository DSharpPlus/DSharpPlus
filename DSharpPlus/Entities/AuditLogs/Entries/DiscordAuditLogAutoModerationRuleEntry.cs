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

using System.Collections.Generic;

public sealed class DiscordAuditLogAutoModerationRuleEntry : DiscordAuditLogEntry
{

    /// <summary>
    /// Id of the rule
    /// </summary>
    public PropertyChange<ulong?> RuleId { get; internal set; }

    /// <summary>
    /// Id of the guild where the rule was changed
    /// </summary>
    public PropertyChange<ulong?> GuildId { get; internal set; }

    /// <summary>
    /// Name of the rule
    /// </summary>
    public PropertyChange<string?> Name { get; internal set; }

    /// <summary>
    /// Id of the user that created the rule
    /// </summary>
    public PropertyChange<ulong?> CreatorId { get; internal set; }

    /// <summary>
    /// Indicates in what event context a rule should be checked.
    /// </summary>
    public PropertyChange<DiscordRuleEventType?> EventType { get; internal set; }

    /// <summary>
    /// Characterizes the type of content which can trigger the rule.
    /// </summary>
    public PropertyChange<DiscordRuleTriggerType?> TriggerType { get; internal set; }

    /// <summary>
    /// Additional data used to determine whether a rule should be triggered. 
    /// </summary>
    public PropertyChange<DiscordRuleTriggerMetadata?> TriggerMetadata { get; internal set; }

    /// <summary>
    /// Actions which will execute when the rule is triggered.
    /// </summary>
    public PropertyChange<IEnumerable<DiscordAutoModerationAction>?> Actions { get; internal set; }

    /// <summary>
    /// Whether the rule is enabled or not.
    /// </summary>
    public PropertyChange<bool?> Enabled { get; internal set; }

    /// <summary>
    /// Roles that should not be affected by the rule
    /// </summary>
    public PropertyChange<IEnumerable<DiscordRole>?> ExemptRoles { get; internal set; }

    /// <summary>
    /// Channels that should not be affected by the rule
    /// </summary>
    public PropertyChange<IEnumerable<DiscordChannel>?> ExemptChannels { get; internal set; }

    /// <summary>
    /// List of trigger keywords that were added to the rule
    /// </summary>
    public IEnumerable<string>? AddedKeywords { get; internal set; }

    /// <summary>
    /// List of trigger keywords that were removed from the rule
    /// </summary>
    public IEnumerable<string>? RemovedKeywords { get; internal set; }

    /// <summary>
    /// List of trigger regex patterns that were added to the rule
    /// </summary>
    public IEnumerable<string>? AddedRegexPatterns { get; internal set; }

    /// <summary>
    /// List of trigger regex patterns that were removed from the rule
    /// </summary>
    public IEnumerable<string>? RemovedRegexPatterns { get; internal set; }

    /// <summary>
    /// List of strings that were added to the allow list
    /// </summary>
    public IEnumerable<string>? AddedAllowList { get; internal set; }

    /// <summary>
    /// List of strings that were removed from the allow list
    /// </summary>
    public IEnumerable<string>? RemovedAllowList { get; internal set; }
}
