// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IOnboardingPrompt" />
public sealed record OnboardingPrompt : IOnboardingPrompt
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required DiscordGuildOnboardingPromptType Type { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<IOnboardingPromptOption> Options { get; init; }

    /// <inheritdoc/>
    public required string Title { get; init; }

    /// <inheritdoc/>
    public required bool SingleSelect { get; init; }

    /// <inheritdoc/>
    public required bool Required { get; init; }

    /// <inheritdoc/>
    public required bool InOnboarding { get; init; }
}