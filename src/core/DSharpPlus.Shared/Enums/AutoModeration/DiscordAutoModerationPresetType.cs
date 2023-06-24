// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Specifies the internal keyword presets for auto-moderation rules of type
/// <seealso cref="DiscordAutoModerationTriggerType.KeywordPreset"/>.
/// </summary>
public enum DiscordAutoModerationPresetType
{
    /// <summary>
    /// Contains words that may be considered swearing, cursing or other profanity.
    /// </summary>
    Profanity = 1,

    /// <summary>
    /// Contains words that may refer to sexually explicit behaviour or activity.
    /// </summary>
    SexualContent,

    /// <summary>
    /// Contains words that may be considered hate speech.
    /// </summary>
    Slurs
}
