// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Internal.Abstractions.Rest.Queries;
using DSharpPlus.Internal.Abstractions.Rest.Responses;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to channel-related rest API calls.
/// </summary>
public interface IChannelRestAPI
{
    /// <summary>
    /// Returns a channel object for the given ID. If the channel is a thread channel, a
    /// <see cref="IThreadMember"/> object is included in the returned channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IChannel>> GetChannelAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies a group DM channel with the given parameters.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the group DM in question.</param>
    /// <param name="payload">Payload object containing the modification parameters.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The modified channel object.</returns>
    public ValueTask<Result<IChannel>> ModifyChannelAsync
    (
        Snowflake channelId,
        IModifyGroupDMPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies a guild channel with the given parameters.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="payload">Payload object containing the modification parameters.</param>
    /// <param name="reason">Optional audit log reason for the edit.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The modified channel object.</returns>
    public ValueTask<Result<IChannel>> ModifyChannelAsync
    (
        Snowflake channelId,
        IModifyGuildChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies a thread channel with the given parameters.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="payload">Payload object containing the modification parameters.</param>
    /// <param name="reason">Optional audit log reason for the edit.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The modified channel object.</returns>
    public ValueTask<Result<IChannel>> ModifyChannelAsync
    (
        Snowflake channelId,
        IModifyThreadChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes a channel. Deleting guild channels cannot be undone. DM channels, however, cannot be deleted
    /// and are restored by opening a direct message channel again.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="reason">Optional audit log reason if this is a guild channel.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The associated channel object.</returns>
    public ValueTask<Result<IChannel>> DeleteChannelAsync
    (
        Snowflake channelId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

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
        ForwardsPaginatedQuery query = default,
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

    /// <summary>
    /// Edits a permission overwrite for a guild channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier for the channel in question.</param>
    /// <param name="overwriteId">The snowflake identifier of the entity (role/user) this overwrite targets.</param>
    /// <param name="payload">The overwrite data to apply.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the overwrite was successfully edited.</returns>
    public ValueTask<Result> EditChannelPermissionsAsync
    (
        Snowflake channelId,
        Snowflake overwriteId,
        IEditChannelPermissionsPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of invite objects with invite metadata pointing to this channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IInvite>>> GetChannelInvitesAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates an invite on the specified channel.
    /// </summary>
    /// <param name="channelId">Snowflake identifier of the channel in question.</param>
    /// <param name="payload">A payload containing information on the invite.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created invite object.</returns>
    public ValueTask<Result<IInvite>> CreateChannelInviteAsync
    (
        Snowflake channelId,
        ICreateChannelInvitePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes a channel permission overwrite.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="overwriteId">The snowflake identifier of the object this overwrite points to.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the deletion was successful.</returns>
    public ValueTask<Result> DeleteChannelPermissionAsync
    (
        Snowflake channelId,
        Snowflake overwriteId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Follows an announcement channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the news channel to follow.</param>
    /// <param name="payload">
    /// The payload, containing the snowflake identifier of the channel you want messages to be cross-posted into.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The created followed channel object.</returns>
    public ValueTask<Result<IFollowedChannel>> FollowAnnouncementChannelAsync
    (
        Snowflake channelId,
        IFollowAnnouncementChannelPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Triggers the typing indicator for the current user in the given channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the channel in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> TriggerTypingIndicatorAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns all pinned messages as message objects.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the messages' parent channel.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IMessage>>> GetPinnedMessagesAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Pins a message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the message was successfully pinned.</returns>
    public ValueTask<Result> PinMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Unpins a message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the message's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the message in question.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the message was successfully unpinned.</returns>
    public ValueTask<Result> UnpinMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Adds the given user to a specified group DM channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the group DM channel in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="payload">Request payload, containing the access token needed.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> GroupDMAddRecipientAsync
    (
        Snowflake channelId,
        Snowflake userId,
        IGroupDMAddRecipientPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Removes the given user from the given group DM channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the group DM channel in question.</param>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> GroupDMRemoveRecipientAsync
    (
        Snowflake channelId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new thread channel from the given message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the thread's parent channel.</param>
    /// <param name="messageId">The snowflake identifier of the thread's parent message.</param>
    /// <param name="payload">Request payload for this request.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created thread channel.</returns>
    public ValueTask<Result<IChannel>> StartThreadFromMessageAsync
    (
        Snowflake channelId,
        Snowflake messageId,
        IStartThreadFromMessagePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new thread channel without a message.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the thread's parent channel.</param>
    /// <param name="payload">Request payload for this request.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created thread channel.</returns>
    public ValueTask<Result<IChannel>> StartThreadWithoutMessageAsync
    (
        Snowflake channelId,
        IStartThreadWithoutMessagePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new thread with a starting message in a forum channel.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the parent forum channel.</param>
    /// <param name="payload">
    /// A payload object for starting a thread from a message containing a <see cref="IForumAndMediaThreadMessage"/>.
    /// A new message is created, then a thread is started from it.
    /// </param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created thread channel.</returns>
    public ValueTask<Result<IChannel>> StartThreadInForumOrMediaChannelAsync
    (
        Snowflake channelId,
        IStartThreadInForumOrMediaChannelPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Joins the current user into a thread.
    /// </summary>
    /// <param name="threadId">The snowflake identifier of the thread channel to be joined.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the operation was successful.</returns>
    public ValueTask<Result> JoinThreadAsync
    (
        Snowflake threadId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Adds another member into a thread.
    /// </summary>
    /// <param name="threadId">The snowflake identifier of the thread to be joined.</param>
    /// <param name="userId">The snowflake identifier of the user to join into the thread.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the operation was successful.</returns>
    public ValueTask<Result> AddThreadMemberAsync
    (
        Snowflake threadId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Leaves a thread as the current bot.
    /// </summary>
    /// <param name="threadId">The snowflake identifier of the thread to be left.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the operation was successful.</returns>
    public ValueTask<Result> LeaveThreadAsync
    (
        Snowflake threadId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Removes another user from a thread.
    /// </summary>
    /// <param name="threadId">The snowflake identifier of the thread to be left.</param>
    /// <param name="userId">The snowflake identifier of the user to be removed.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the operation was successful.</returns>
    public ValueTask<Result> RemoveThreadMemberAsync
    (
        Snowflake threadId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a thread member object for the specified user.
    /// </summary>
    /// <param name="threadId">The snowflake identifier of the thread to obtain data from.</param>
    /// <param name="userId">The snowflake identifier of the user to obtain data for.</param>
    /// <param name="query">
    /// Specifies whether the returned thread member object should contain guild member data.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IThreadMember>> GetThreadMemberAsync
    (
        Snowflake threadId,
        Snowflake userId,
        GetThreadMemberQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of all thread members for the specified thread.
    /// </summary>
    /// <param name="threadId">The snowflake identifier fo the thread to obtain data from.</param>
    /// <param name="query">
    /// Specifies additional query information pertaining to pagination and optional additional data.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IThreadMember>>> ListThreadMembersAsync
    (
        Snowflake threadId,
        ListThreadMembersQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns all public, archived threads for this channel including respective thread member objects.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the thread's parent channel.</param>
    /// <param name="query">Contains pagination information for this request.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<ListArchivedThreadsResponse>> ListPublicArchivedThreadsAsync
    (
        Snowflake channelId,
        ListArchivedThreadsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns all private, accessible, archived threads for this channel including respective thread member objects.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the thread's parent channel.</param>
    /// <param name="query">Contains pagination information for this request.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<ListArchivedThreadsResponse>> ListPrivateArchivedThreadsAsync
    (
        Snowflake channelId,
        ListArchivedThreadsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of joined, private, archived threads.
    /// </summary>
    /// <param name="channelId">The nowflake identifier of their parent channel.</param>
    /// <param name="query">Contains pagination information for this request.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<ListArchivedThreadsResponse>> ListJoinedPrivateArchivedThreadsAsync
    (
        Snowflake channelId,
        ListJoinedPrivateArchivedThreadsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}

