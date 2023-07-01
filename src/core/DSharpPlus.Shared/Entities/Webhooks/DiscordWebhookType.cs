// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Specifies the different types of webhooks.
/// </summary>
public enum DiscordWebhookType
{
    /// <summary>
    /// Incoming webhooks can post messages to channels with a generated token.
    /// </summary>
    Incoming = 1,

    /// <summary>
    /// Channel Follower webhooks are internal webhooks used with channel following to cross-post messages.
    /// </summary>
    ChannelFollower,

    /// <summary>
    /// Application webhooks are webhooks used with interactions.
    /// </summary>
    Application
}
