// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to message-related rest API calls.
/// </summary>
public interface IMessageRestAPI
{
    /// <summary>
    /// Returns a set amount of messages, optionally before, after or around a certain message.
    /// </summary>
    /// <remarks>
    /// <c>around</c>, <c>before</c> and <c>after</c> are mutually exclusive. Only one may be passed. If multiple are passed,
    /// only the first one in the parameter list is respected, independent of the order they are passed in client code.
    /// </remarks>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="query">Specifies where to get messages from, used for paginating.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IMessage>>> GetChannelMessagesAsync
    (
        Snowflake channelId,
        GetChannelMessagesQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Gets a message by its snowflake identifier.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IMessage>> GetChannelMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new message in a channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's target channel.</param>
    /// <param name="payload">Message creation payload including potential attachment files.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created message object.</returns>
    public ValueTask<Result<IMessage>> CreateMessageAsync
    (
        Snowflake channelId,
        ICreateMessagePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Publishes a message in an announcement channel to following channels.
    /// </summary>
    /// <param name="channelId">The origin announcement channel of this message.</param>
    /// <param name="messageId">The snowflake identifier of the message.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IMessage>> CrosspostMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a reaction with the given emoji on the specified message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="emoji">The string representation of the emoji.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the reaction was added successfully.</returns>
    public ValueTask<Result> CreateReactionAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes your own reaction with the specified emoji on the specified message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="emoji">The string representation of the emoji.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the reaction was removed successfully.</returns>
    public ValueTask<Result> DeleteOwnReactionAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the specified user's reaction with the specified emoji on the specified message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="emoji">The string representation of the emoji.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the reaction was removed successfully.</returns>
    public ValueTask<Result> DeleteUserReactionAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        Snowflake userId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Gets a list of users that reacted with the given emoji.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="emoji">The string representation of the queried emoji.</param>
    /// <param name="query">Contains query information related to request pagination.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IUser>>> GetReactionsAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        GetReactionsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes all reactions on the given message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteAllReactionsAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes all reactions with a specific emoji from the specified message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="emoji">The string representation of the emoji in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteAllReactionsForEmojiAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string emoji,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Edits the given message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="payload">A payload object containing information on how to edit this message.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IMessage>> EditMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        IEditMessagePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes a given message.
    /// </summary>
    /// <param name="channelId">The nowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the message was successfully deleted.</returns>
    public ValueTask<Result> DeleteMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Bulk-deletes the provided messages.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="payload">
    /// Up to 100 message IDs to delete. If any messages older than two weeks are included,
    /// or any of the IDs are duplicated, the entire request will fail.
    /// </param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the messages were deleted successfully.</returns>
    public ValueTask<Result> BulkDeleteMessagesAsync
    (
        Snowflake channelId,
        IBulkDeleteMessagesPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
