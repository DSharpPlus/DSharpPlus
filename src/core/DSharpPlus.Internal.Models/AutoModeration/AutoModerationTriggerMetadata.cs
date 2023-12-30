// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IAutoModerationTriggerMetadata" />
public sealed record AutoModerationTriggerMetadata : IAutoModerationTriggerMetadata
{
    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> KeywordFilter { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> RegexPatterns { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<DiscordAutoModerationPresetType>> Presets { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> AllowList { get; init; }

    /// <inheritdoc/>
    public Optional<int> MentionTotalLimit { get; init; }

    /// <inheritdoc/>
    public Optional<bool> MentionRaidProtectionEnabled { get; init; }
}