// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /webhooks/:webhook-id</c>.
/// </summary>
public interface IModifyWebhookPayload
{
    /// <summary>
    /// The new default name of this webhook.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The new default webhook avatar image.
    /// </summary>
    public Optional<InlineMediaData?> Avatar { get; }

    /// <summary>
    /// The snowflake identifier of the channel this webhook should be moved to.
    /// </summary>
    public Optional<Snowflake> ChannelId { get; }
}
