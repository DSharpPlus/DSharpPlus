// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

using OneOf;

namespace DSharpPlus.Extensions.Internal.Builders.Implementations;

/// <inheritdoc cref="IInteractionResponse" />
internal sealed record BuiltInteractionResponse : IInteractionResponse
{
    /// <inheritdoc/>
    public required DiscordInteractionCallbackType Type { get; init; }

    /// <inheritdoc/>
    public Optional<OneOf<IAutocompleteCallbackData, IMessageCallbackData, IModalCallbackData>> Data { get; init; }
}
