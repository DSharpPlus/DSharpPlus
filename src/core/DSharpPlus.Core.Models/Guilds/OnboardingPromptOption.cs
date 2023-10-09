// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IOnboardingPromptOption" />
public sealed record OnboardingPromptOption : IOnboardingPromptOption
{
    /// <inheritdoc/>
    public required Snowflake Id { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> ChannelIds { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> RoleIds { get; init; }

    /// <inheritdoc/>
    public required IEmoji Emoji { get; init; }

    /// <inheritdoc/>
    public required string Title { get; init; }

    /// <inheritdoc/>
    public string? Description { get; init; }
}