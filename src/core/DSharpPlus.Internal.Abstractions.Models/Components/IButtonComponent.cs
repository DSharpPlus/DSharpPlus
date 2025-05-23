// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a button attached to a message.
/// </summary>
public interface IButtonComponent : IComponent
{
    /// <summary>
    /// The type of this component.
    /// </summary>
    public DiscordMessageComponentType Type { get; }

    /// <summary>
    /// An optional numeric identifier for this component.
    /// </summary>
    public Optional<int> Id { get; }

    /// <summary>
    /// The visual style of this button.
    /// </summary>
    public DiscordButtonStyle Style { get; }

    /// <summary>
    /// The text to render on this button, up to 80 characters.
    /// </summary>
    public Optional<string> Label { get; }

    /// <summary>
    /// The emoji to render on this button, if any.
    /// </summary>
    public Optional<IPartialEmoji> Emoji { get; }

    /// <summary>
    /// A developer-defined identifier for this button, up to 100 characters. This is mutually
    /// exclusive with <see cref="Url"/>.
    /// </summary>
    public Optional<string> CustomId { get; }

    /// <summary>
    /// An URL for link-style buttons. This is mutually exclusive with <see cref="CustomId"/>.
    /// </summary>
    public Optional<string> Url { get; }

    /// <summary>
    /// Indicates whether this button is disabled, default false.
    /// </summary>
    public Optional<bool> Disabled { get; }

    /// <summary>
    /// A snowflake identifier of a purchasable SKU to link to. This is only available on premium buttons.
    /// </summary>
    public Optional<Snowflake> SkuId { get; }
}
