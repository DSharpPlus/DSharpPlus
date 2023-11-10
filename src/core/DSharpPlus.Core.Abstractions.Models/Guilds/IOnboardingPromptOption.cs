// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an option for a guild onboarding prompt.
/// </summary>
public interface IOnboardingPromptOption
{
    /// <summary>
    /// The snowflake identifier of the prompt option.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The snowflake identifiers for channels a member is added to when this option is selected.
    /// </summary>
    public IReadOnlyList<Snowflake> ChannelIds { get; }

    /// <summary>
    /// The snowflake identifiers for roles assigned to a member when this option is selected.
    /// </summary>
    public IReadOnlyList<Snowflake> RoleIds { get; }

    /// <summary>
    /// The emoji for this option. This field is only ever returned, and must not be sent when creating
    /// or updating a prompt option.
    /// </summary>
    public Optional<IEmoji> Emoji { get; }

    /// <summary>
    /// The snowflake identifier of the emoji for this option.
    /// </summary>
    public Optional<Snowflake> EmojiId { get; }

    /// <summary>
    /// The name of the emoji for this option.
    /// </summary>
    public Optional<string> EmojiName { get; }

    /// <summary>
    /// Indicates whether the emoji for this option is animated.
    /// </summary>
    public Optional<bool> EmojiAnimated { get; }

    /// <summary>
    /// The title of this option.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// The description of this option.
    /// </summary>
    public string? Description { get; }
}
