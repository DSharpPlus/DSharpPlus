// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Rest.Payloads;

/// <inheritdoc cref="IModifyApplicationEmojiPayload" />
public sealed record ModifyApplicationEmojiPayload : IModifyApplicationEmojiPayload
{
    /// <inheritdoc/>
    public required string Name { get; init; }
}