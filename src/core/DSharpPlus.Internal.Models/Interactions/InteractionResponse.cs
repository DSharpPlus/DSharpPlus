// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

using OneOf;

namespace DSharpPlus.Internal.Models;

/// <inheritdoc cref="IInteractionResponse" />
public sealed record InteractionResponse : IInteractionResponse
{
    /// <inheritdoc/>
    public required DiscordInteractionCallbackType Type { get; init; }

    /// <inheritdoc/>
    public Optional<OneOf<IAutocompleteCallbackData, IMessageCallbackData, IModalCallbackData>> Data { get; init; }
}