// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents an user prompt during the onboarding flow.
/// </summary>
public interface IOnboardingPrompt
{
    /// <summary>
    /// The snowflake identifier of the prompt option.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The type of this prompt.
    /// </summary>
    public DiscordGuildOnboardingPromptType Type { get; }

    /// <summary>
    /// The options available within this prompt.
    /// </summary>
    public IReadOnlyList<IOnboardingPromptOption> Options { get; }

    /// <summary>
    /// The title of this prompt.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Indicates whether users are limited to selecting only one of the <see cref="Options"/>.
    /// </summary>
    public bool SingleSelect { get; }

    /// <summary>
    /// Indicates whether this prompt is required to be completed before the user can complete the onboarding flow.
    /// </summary>
    public bool Required { get; }

    /// <summary>
    /// Indicates whether this prompt is present in the onboarding flow. If <see langword="false"/>, the
    /// prompt will only appear in Customize Community.
    /// </summary>
    public bool InOnboarding { get; }
}
