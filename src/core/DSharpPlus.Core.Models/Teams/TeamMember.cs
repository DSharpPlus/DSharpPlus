// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="ITeamMember" />
public sealed record TeamMember : ITeamMember
{
    /// <inheritdoc/>
    public required DiscordTeamMembershipState MembershipState { get; init; }

    /// <inheritdoc/>
    public required IReadOnlyList<string> Permissions { get; init; }

    /// <inheritdoc/>
    public required Snowflake TeamId { get; init; }

    /// <inheritdoc/>
    public required IPartialUser User { get; init; }
}