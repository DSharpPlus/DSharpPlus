// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /webhooks/:webhook-id/:webhook-token</c>.
/// </summary>
public interface IModifyWebhookWithTokenPayload
{
    /// <summary>
    /// The new default name of this webhook.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The new default webhook avatar image.
    /// </summary>
    public Optional<ImageData?> Avatar { get; }
}
