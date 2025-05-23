// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters to <c>IWebhookRestAPI.EditWebhookMessageAsync</c>. 
/// </summary>
public readonly record struct EditWebhookMessageQuery
{

    /// <summary>
    /// Specifies a thread to edit the message in rather than directly in the parent channel. If the thread 
    /// is archived, this will automatically unarchive it. Only threads with the same parent channel as the 
    /// webhook can be passed.
    /// </summary>
    public Snowflake? ThreadId { get; init; }

    /// <summary>
    /// Specifies whether this request will allow sending non-interactive components for non-application-owned webhooks.
    /// Defaults to <see langword="false"/>
    /// </summary>
    public bool? WithComponents { get; init; }
}
