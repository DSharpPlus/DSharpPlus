// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents metadata about an interaction a message was sent in response to.
/// </summary>
public interface IMessageInteractionMetadata
{
    /// <summary>
    /// The snowflake identifier of the interaction.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The type of this interaction.
    /// </summary>
    public DiscordInteractionType Type { get; }

    /// <summary>
    /// The snowflake identifier of the user who triggered this interaction.
    /// </summary>
    public Snowflake UserId { get; }

    /// <summary>
    /// The installation contexts related to this interaction.
    /// </summary>
    public IReadOnlyDictionary<DiscordApplicationIntegrationType, Snowflake> AuthorizingIntegrationOwners { get; }

    /// <summary>
    /// The snowflake identifier of the original response to the interaction. This is only present on follow-up responses.
    /// </summary>
    public Optional<Snowflake> OriginalResponseMessageId { get; }

    /// <summary>
    /// If this interaction was created from a component, this is the snowflake identifier of the message containing the component.
    /// </summary>
    public Optional<Snowflake> InteractedMessageId { get; }

    /// <summary>
    /// If this message was sent in response to a modal interaction, this specifies metadata for the parent interaction the modal
    /// was created in response to.
    /// </summary>
    public Optional<IMessageInteractionMetadata> TriggeringInteractionMetadata { get; }
}
