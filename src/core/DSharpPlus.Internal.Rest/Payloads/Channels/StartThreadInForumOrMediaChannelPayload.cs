// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IStartThreadInForumOrMediaChannelPayload" />
public sealed record StartThreadInForumOrMediaChannelPayload : IStartThreadInForumOrMediaChannelPayload
{
    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public Optional<int> AutoArchiveDuration { get; init; }

    /// <inheritdoc/>
    public Optional<int?> RateLimitPerUser { get; init; }

    /// <inheritdoc/>
    public required IForumAndMediaThreadMessage Message { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<Snowflake>> AppliedTags { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<AttachmentData>? Files { get; init; }
}
