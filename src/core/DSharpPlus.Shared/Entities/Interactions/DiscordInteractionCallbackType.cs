// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Represents the different ways of responding to different interactions.
/// </summary>
public enum DiscordInteractionCallbackType
{
    /// <summary>
    /// Acknowledges a <seealso cref="DiscordInteractionType.Ping"/>.
    /// </summary>
    Pong = 1,

    /// <summary>
    /// Responds to an interaction by sending a message.
    /// </summary>
    ChannelMessageWithSource = 4,

    /// <summary>
    /// Acknowledges an interaction and allows the bot to edit a response in later; the user
    /// sees a loading state.
    /// </summary>
    DeferredChannelMessageWithSource,

    /// <summary>
    /// Acknowledges a component interaction and allows the bot to edit a response in later;
    /// the user does not see a loading state.
    /// </summary>
    DeferredUpdateMessage,

    /// <summary>
    /// Responds to a component interaction by editing the message the component was attached to.
    /// </summary>
    UpdateMessage,

    /// <summary>
    /// Responds to an autocomplete interaction by suggesting choices.
    /// </summary>
    ApplicationCommandAutocompleteResult,

    /// <summary>
    /// Responds to an interaction with a pop-up modal. This cannot be sent in response to a modal
    /// submission or a ping.
    /// </summary>
    Modal
}
