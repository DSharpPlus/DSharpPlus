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
/// Provides access to guild-scheduled-event-related rest API calls.
/// </summary>
public interface IGuildScheduledEventRestAPI
{
    /// <summary>
    /// Returns a list of scheduled events taking place in this guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="query">Specifies whether the answer should include user counts.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IScheduledEvent>>> ListScheduledEventsForGuildAsync
    (
        Snowflake guildId,
        WithUserCountQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new scheduled event in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The data to intialize the event with.</param>
    /// <param name="reason">An optional audit log reason</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created scheduled event.</returns>
    public ValueTask<Result<IScheduledEvent>> CreateGuildScheduledEventAsync
    (
        Snowflake guildId,
        ICreateGuildScheduledEventPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the requested scheduled event.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild this scheduled event takes place in.</param>
    /// <param name="eventId">The snowflake identifier of the scheduled event in qeustion.</param>
    /// <param name="query">
    /// Specifies whether the number of users subscribed to this event should be included.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IScheduledEvent>> GetScheduledEventAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        WithUserCountQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the given scheduled event.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild this event takes place in.</param>
    /// <param name="eventId">The snowflake identifier of the event to be modified.</param>
    /// <param name="payload">The new information for this event.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly modified scheduled event.</returns>
    public ValueTask<Result<IScheduledEvent>> ModifyScheduledEventAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        IModifyGuildScheduledEventPayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the given scheduled event.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild this event takes place in.</param>
    /// <param name="eventId">The snowflake identifier of the event to be modified.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the deletion was successful.</returns>
    public ValueTask<Result> DeleteScheduledEventAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns <see cref="IScheduledEventUser"/> objects for each participant of the given scheduled event.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild this scheduled event belongs to.</param>
    /// <param name="eventId">The snowflake identifier of the scheduled event in question.</param>
    /// <param name="query">Additional information regarding request pagination and member data in the response.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IScheduledEventUser>>> GetScheduledEventUsersAsync
    (
        Snowflake guildId,
        Snowflake eventId,
        GetScheduledEventUsersQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
