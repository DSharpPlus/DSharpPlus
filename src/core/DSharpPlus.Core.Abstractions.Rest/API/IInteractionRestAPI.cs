// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Abstractions.Rest.Payloads;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to interaction-related rest API calls.
/// </summary>
public interface IInteractionRestAPI
{
    /// <summary>
    /// Creates a response to an interaction from the gateway.
    /// </summary>
    /// <param name="interactionId">The snowflake identifier of the interaction.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="payload">The response to this interaction.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> CreateInteractionResponseAsync
    (
        Snowflake interactionId,
        string interactionToken,
        IInteractionResponse payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Gets the original response to this interaction, if it was a message.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IMessage>> GetInteractionResponseAsync
    (
        Snowflake applicationId,
        string interactionToken,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Edits the original response to this interaction, if it was a message.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="payload">The message editing payload.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly edited message.</returns>
    public ValueTask<Result<IMessage>> EditInteractionResponseAsync
    (
        Snowflake applicationId,
        string interactionToken,
        IEditInteractionResponsePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the original response to this interaction, if it was a message.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteInteractionResponseAsync
    (
        Snowflake applicationId,
        string interactionToken,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a followup message for an interaction. If this is the first followup to a deferred interaction
    /// response as created by 
    /// <seealso cref="CreateInteractionResponseAsync(Snowflake, string, IInteractionResponse, RequestInfo, CancellationToken)"/>,
    /// ephemerality of this message will be dictated by the <seealso cref="IInteractionResponse"/> supplied 
    /// originally instead of <seealso cref="ICreateFollowupMessagePayload.Flags"/>.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="payload">A payload containing data to create a message from.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created followup message.</returns>
    public ValueTask<Result<IMessage>> CreateFollowupMessageAsync
    (
        Snowflake applicationId,
        string interactionToken,
        ICreateFollowupMessagePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Fetches a followup message created for this interaction.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="interactionToken">The interaction token received with the interaction.</param>
    /// <param name="messageId">The snowflake identifier of the followup message.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IMessage>> GetFollowupMessageAsync
    (
        Snowflake applicationId,
        string interactionToken,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
