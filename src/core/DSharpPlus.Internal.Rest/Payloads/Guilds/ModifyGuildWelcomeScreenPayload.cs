// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyGuildWelcomeScreenPayload" />
public sealed record ModifyGuildWelcomeScreenPayload : IModifyGuildWelcomeScreenPayload
{
    /// <inheritdoc/>
    public Optional<bool?> Enabled { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<IWelcomeScreenChannel>?> WelcomeChannels { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Description { get; init; }
}