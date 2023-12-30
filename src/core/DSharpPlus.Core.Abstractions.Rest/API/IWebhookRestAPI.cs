// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Core.Abstractions.Rest.Payloads;
using DSharpPlus.Core.Abstractions.Rest.Queries;
using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to webhook-related API calls.
/// </summary>
public interface IWebhookRestAPI
{
    /// <summary>
    /// Creates a new webhook in the specified channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="payload">Request payload.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created webhook object.</returns>
    public ValueTask<Result<IWebhook>> CreateWebhookAsync
    (
        Snowflake channelId,
        ICreateWebhookPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of channel webhook objects.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IWebhook>>> GetChannelWebhooksAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of guild webhook objects.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IWebhook>>> GetGuildWebhooksAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the specified webhook object.
    /// </summary>
    /// <param name="webhookId">The snowflake identifier of the webhook in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IWebhook>> GetWebhookAsync
    (
        Snowflake webhookId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the specified webhook object.
    /// </summary>
    /// <remarks>
    /// This endpoint does not require authentication and is not counted to your global ratelimits.
    /// </remarks>
    /// <param name="webhookId">The snowflake identifier of the webhook in question.</param>
    /// <param name="webhookToken">The access token to this webhook.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IWebhook>> GetWebhookWithTokenAsync
    (
        Snowflake webhookId,
        string webhookToken,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the given webhook.
    /// </summary>
    /// <param name="webhookId">The snowflake identifier of the webhook to edit.</param>
    /// <param name="payload">The information to modify this webhook with.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The updated webhook object.</returns>
    public ValueTask<Result<IWebhook>> ModifyWebhookAsync
    (
        Snowflake webhookId,
        IModifyWebhookPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the given webhook.
    /// </summary>
    /// <remarks>
    /// This endpoint does not require authentication and is not counted to your global ratelimits.
    /// </remarks>
    /// <param name="webhookId">The snowflake identifier of the webhook to edit.</param>
    /// <param name="webhookToken">The webhook token of the webhook to edit.</param>
    /// <param name="payload">The information to modify this webhook with.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The updated webhook object.</returns>
    public ValueTask<Result<IWebhook>> ModifyWebhookWithTokenAsync
    (
        Snowflake webhookId,
        string webhookToken,
        IModifyWebhookWithTokenPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the given webhook.
    /// </summary>
    /// <param name="webhookId">The snowflake identifier of the webhook to delete.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteWebhookAsync
    (
        Snowflake webhookId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the given webhook.
    /// </summary>
    /// <remarks>
    /// This endpoint does not require authentication and is not counted to your global ratelimits.
    /// </remarks>
    /// <param name="webhookId">The snowflake identifier of the webhook to delete.</param>
    /// <param name="webhookToken">The webhook token of the webhook to delete.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteWebhookWithTokenAsync
    (
        Snowflake webhookId,
        string webhookToken,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Executes the given webhook.
    /// </summary>
    /// <param name="webhookId">The snowflake identifier of the webhook to execute.</param>
    /// <param name="webhookToken">The webhook token of the webhook to execute.</param>
    /// <param name="payload">A payload of information on the message to send.</param>
    /// <param name="query">
    ///	Specifies the waiting behaviour of the request as well as whether this webhook should post to a thread.
    ///	</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>
    /// If <seealso name="ExecuteWebhookQuery.Wait"/> was set to <see langword="true"/>, a 
    /// <seealso cref="IMessage"/> object. If <seealso name="ExecuteWebhookQuery.Wait"/> was set to 
    /// <see langword="false"/>, <see langword="null"/>.
    /// </returns>
    public ValueTask<Result<IMessage?>> ExecuteWebhookAsync
    (
        Snowflake webhookId,
        string webhookToken,
        IExecuteWebhookPayload payload,
        ExecuteWebhookQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a previously-sent webhook message from the same token.
    /// </summary>
    /// <param name="webhookId">The snowflake identifier of your webhook.</param>
    /// <param name="webhookToken">
    /// The webhook token for your webhook. This must match the token of the original author.
    /// </param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="query">
    ///	Specifies the thread to search in rather than the parent channel. Only threads with the same parent channel
    ///	as the webhook can be passed.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IMessage>> GetWebhookMessageAsync
    (
        Snowflake webhookId,
        string webhookToken,
        Snowflake messageId,
        ThreadIdQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Edits a previously-sent webhook message from the same token.
    /// </summary>
    /// <param name="webhookId">The snowflake identifier of your webhook.</param>
    /// <param name="webhookToken">
    /// The webhook token for your webhook. This must match the token of the original author.
    /// </param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="payload">The information to update this message with.</param>
    /// <param name="query">
    ///	Specifies the thread to search in rather than the parent channel. Only threads with the same parent channel
    ///	as the webhook can be passed.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly edited message.</returns>
    public ValueTask<Result<IMessage>> EditWebhookMessageAsync
    (
        Snowflake webhookId,
        string webhookToken,
        Snowflake messageId,
        IEditWebhookMessagePayload payload,
        ThreadIdQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes a previously-sent webhook message from the same token.
    /// </summary>
    /// <param name="webhookId">The snowflake identifier of your webhook.</param>
    /// <param name="webhookToken">
    /// The webhook token for your webhook. This must match the token of the original author.
    /// </param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="query">
    ///	Specifies the thread to search in rather than the parent channel. Only threads with the same parent channel
    ///	as the webhook can be passed.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteWebhookMessageAsync
    (
        Snowflake webhookId,
        string webhookToken,
        Snowflake messageId,
        ThreadIdQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
