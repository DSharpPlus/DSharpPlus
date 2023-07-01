// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Represents different automod triggers.
/// </summary>
public enum DiscordAutoModerationTriggerType
{
    /// <summary>
    /// Check whether the message content contains words from a user-defined list of keywords.
    /// There can be up to six such rules in a guild.
    /// </summary>
    Keyword = 1,

    /// <summary>
    /// Check whether the message content represents generic spam.
    /// </summary>
    Spam = 3,

    /// <summary>
    /// Check whether the message content contains words from internally defined wordsets.
    /// </summary>
    KeywordPreset,

    /// <summary>
    /// Check whether the message content contains more unique mentions than allowed.
    /// </summary>
    MentionSpam
}
