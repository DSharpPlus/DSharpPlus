// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents the guild onboarding flow for the given guild.
/// </summary>
public interface IOnboarding
{
    /// <summary>
    /// The snowflake identifier of the guild this onboarding is part of.
    /// </summary>
    public Snowflake GuildId { get; }

    /// <summary>
    /// Prompts shown during onboarding and in Customize Community
    /// </summary>
    public IReadOnlyList<IOnboardingPrompt> Prompts { get; }

    /// <summary>
    /// Snowflake identifiers of channels that members are "opted into" automatically.
    /// </summary>
    public IReadOnlyList<Snowflake> DefaultChannelIds { get; }

    /// <summary>
    /// Indicates whether onboarding is enabled in the guild.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// The current onboarding mode.
    /// </summary>
    public DiscordGuildOnboardingPromptType Mode { get; }
}
