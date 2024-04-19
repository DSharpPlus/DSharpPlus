// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the query parameters to <c>IWebhookRestAPI.ExecuteWebhookAsync</c>. 
/// </summary>
public readonly record struct ExecuteWebhookQuery
{
    /// <summary>
    /// Specifies whether to wait for server confirmation. If this is set to true, a <see cref="IMessage"/>
    ///	object will be returned, if not, <see langword="null"/> will be returned on success instead. 
    ///	Defaults to <see langword="false"/>.
    /// </summary>
    public bool? Wait { get; init; }

    /// <summary>
    /// Specifies a thread to send the message to rather than directly to the parent channel. If the thread 
    /// is archived, this will automatically unarchive it. Only threads with the same parent channel as the 
    /// webhook can be passed.
    /// </summary>
    public Snowflake? ThreadId { get; init; }
}
