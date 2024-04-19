// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to poll-related REST API calls.
/// </summary>
public interface IPollRestAPI
{
    /// <summary>
    /// Gets a list of users who voted for the given answer.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel containing this poll.</param>
    /// <param name="messageId">The snowflake identifier of the message containing this poll.</param>
    /// <param name="answerId">The numeric identifier of the answer to query users for.</param>
    /// <param name="query">Pagination info for this request. The default limit is 25, but it may range between 1 and 100.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<GetAnswerVotersResponse>> GetAnswerVotersAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        int answerId,
        ForwardsPaginatedQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Immediately ends the provided poll. You cannot end polls from other users.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel containing this poll.</param>
    /// <param name="messageId">The snowflake identifier of the message containing this poll.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IMessage>> EndPollAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
