// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a text input field in a modal.
/// </summary>
public interface ITextInputComponent : IInteractiveComponent
{
    /// <summary>
    /// The type of this component.
    /// </summary>
    public DiscordMessageComponentType Type { get; }

    /// <summary>
    /// The identifier of this input field, up to 100 characters.
    /// </summary>
    public string CustomId { get; }

    /// <summary>
    /// Indicates whether this input field requests short-form or long-form input.
    /// </summary>
    public DiscordTextInputStyle Style { get; }

    /// <summary>
    /// The label of this input field, up to 45 charcters.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// The minimum length for a text input, between 0 and 4000 characters.
    /// </summary>
    public Optional<int> MinLength { get; }

    /// <summary>
    /// The maximum length for a text input, between 1 and 4000 characters.
    /// </summary>
    public Optional<int> MaxLength { get; }

    /// <summary>
    /// Indicates whether this text input field is required to be filled, defaults to true.
    /// </summary>
    public Optional<bool> Required { get; }

    /// <summary>
    /// A pre-filled value for this component, up to 4000 characters.
    /// </summary>
    public Optional<string> Value { get; }

    /// <summary>
    /// A custom placeholder if the input field is empty, up to 100 characters.
    /// </summary>
    public Optional<string> Placeholder { get; }
}
