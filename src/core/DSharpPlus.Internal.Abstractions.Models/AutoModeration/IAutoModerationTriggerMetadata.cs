// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents additional rule metadata, based on the configured trigger.
/// </summary>
/// <remarks>
/// Which fields may be meaningful depends on the trigger type of the parent rule, see
/// <seealso href="https://discord.com/developers/docs/resources/auto-moderation#auto-moderation-rule-object-trigger-metadata">
/// the documentation</seealso> for additional information.
/// </remarks>
public interface IAutoModerationTriggerMetadata
{
    /// <summary>
    /// Substrings which the automod will search for in message content, up to 1000.
    /// </summary>
    public Optional<IReadOnlyList<string>> KeywordFilter { get; }

    /// <summary>
    /// Rust-flavoured regex patterns which will be matched against message content, up to 10.
    /// </summary>
    public Optional<IReadOnlyList<string>> RegexPatterns { get; }

    /// <summary>
    /// The pre-defined wordsets that will be searched for in message content.
    /// </summary>
    public Optional<IReadOnlyList<DiscordAutoModerationPresetType>> Presets { get; }

    /// <summary>
    /// Substrings which will not trigger the rule, even if otherwise filtered out. The maximum
    /// depends on the parent trigger type.
    /// </summary>
    public Optional<IReadOnlyList<string>> AllowList { get; }

    /// <summary>
    /// The total number of unique role and user mentions allowed per message, up to 50.
    /// </summary>
    public Optional<int> MentionTotalLimit { get; }

    /// <summary>
    /// Indicates whether mention raid detection is enabled.
    /// </summary>
    public Optional<bool> MentionRaidProtectionEnabled { get; }
}
