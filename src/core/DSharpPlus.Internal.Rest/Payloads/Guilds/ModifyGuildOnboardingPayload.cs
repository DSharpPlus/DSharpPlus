// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyGuildOnboardingPayload" />
public sealed record ModifyGuildOnboardingPayload : IModifyGuildOnboardingPayload
{
    /// <inheritdoc/>
    public required IReadOnlyList<IOnboardingPrompt> Prompts { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<Snowflake> DefaultChannelIds { get; init; }

    /// <inheritdoc/>
    public required bool Enabled { get; init; }

    /// <inheritdoc/>
    public required DiscordGuildOnboardingMode Mode { get; init; }
}
