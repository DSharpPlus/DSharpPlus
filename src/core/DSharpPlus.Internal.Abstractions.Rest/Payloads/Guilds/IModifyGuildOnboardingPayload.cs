// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PUT /guilds/:guild-id/onboarding</c>.
/// </summary>
public interface IModifyGuildOnboardingPayload
{
    /// <summary>
    /// Prompts shown during onboarding and in Customize Community.
    /// </summary>
    public IReadOnlyList<IOnboardingPrompt> Prompts { get; }

    /// <summary>
    /// The snowflake identifiers of channels that members get opted into automatically.
    /// </summary>
    public IReadOnlyList<Snowflake> DefaultChannelIds { get; }

    /// <summary>
    /// Indicates whether onboarding is enabled in the guild.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// The current onboarding mode in this guild.
    /// </summary>
    public DiscordGuildOnboardingMode Mode { get; }
}
