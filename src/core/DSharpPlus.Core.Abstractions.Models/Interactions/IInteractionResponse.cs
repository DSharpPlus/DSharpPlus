// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using OneOf;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an outgoing response to an interaction.
/// </summary>
public interface IInteractionResponse
{
    /// <summary>
    /// The type of this response.
    /// </summary>
    public DiscordInteractionCallbackType Type { get; }

    /// <summary>
    /// An additional response message for this interaction.
    /// </summary>
    public Optional<OneOf<IAutocompleteCallbackData, IMessageCallbackData, IModalCallbackData>> Data { get; }
}
