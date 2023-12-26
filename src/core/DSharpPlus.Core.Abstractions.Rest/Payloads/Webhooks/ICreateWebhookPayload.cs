// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /channels/:channel-id/webhooks</c>.
/// </summary>
public interface ICreateWebhookPayload
{
    /// <summary>
    /// The name of the webhook.
    /// </summary>
    /// <remarks>
    /// This follows several requirements:<br/><br/>
    /// <list type="bullet">
    ///		<item>A webhook name can contain up to 80 characters, unlike usernames/nicknames which are limited to 32.</item>
    ///		<item>A webhook name is subject to all other requirements usernames and nicknames are subject to.</item>
    ///		<item>Webhook names cannot contain the substrings <i>clyde</i> and <i>discord</i>, case-insensitively.</item>
    /// </list>
    /// If the name does not fit all three requirements, it will be rejected and an error will be returned.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// The default webhook avatar. This may be overridden by messages.
    /// </summary>
    public Optional<ImageData?> Avatar { get; }
}
