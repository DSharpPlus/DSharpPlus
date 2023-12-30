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
/// Provides access to user-related API calls.
/// </summary>
public interface IUserRestAPI
{
    /// <summary>
    /// Returns the current user.
    /// </summary>
    /// <remarks>
    /// For OAuth2, this requires the <c>identify</c> scope, which will return the object without an email,
    /// and optionally the <c>email</c> scope, which will return the object with an email.
    /// </remarks>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IUser>> GetCurrentUserAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the requested user.
    /// </summary>
    /// <param name="userId">The snowflake identifier of the user in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IUser>> GetUserAsync
    (
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the current user.
    /// </summary>
    /// <param name="payload">The new information for the current user.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly modified current user.</returns>
    public ValueTask<Result<IUser>> ModifyCurrentUserAsync
    (
        IModifyCurrentUserPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of partial guild objects representing the guilds the current user has joined.
    /// </summary>
    /// <remarks>
    /// <seealso cref="GetCurrentUserGuildsQuery.Limit"/> defaults to 200 guilds, which is the maximum 
    /// number of guilds an user account can join. Pagination is therefore not needed for obtaining user 
    /// guilds, but may be needed for obtaining bot guilds.
    /// </remarks>
    /// <param name="query">
    /// Specifies request pagination info, as well as whether guild objects should include member counts.
    /// </param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IPartialGuild>>> GetCurrentUserGuildsAsync
    (
        GetCurrentUserGuildsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a guild member object for the current user for the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IGuildMember>> GetCurrentUserGuildMemberAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Leaves a guild as the current user.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild to be left.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>Whether the operation was successful.</returns>
    public ValueTask<Result> LeaveGuildAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new DM channel with a user.
    /// </summary>
    /// <remarks>
    /// As per Discord's documentation, you should not use this endpoint to DM everyone in a server about
    /// something. DMs should generally be initiated by user action. If you open a significant amount of DMs
    /// too quickly, your bot may be rate limited or blocked from opening new ones.
    /// </remarks>
    /// <param name="payload">The identifier of the user you want to create a DM with.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created channel object, or the existing DM channel if one existed.</returns>
    public ValueTask<Result<IChannel>> CreateDmAsync
    (
        ICreateDmPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new group DM with multiple users. This is limited to 10 active group DMs.
    /// </summary>
    /// <param name="payload">The access tokens and nicks of the users to add.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created channel, or the existing DM channel if one existed.</returns>
    public ValueTask<Result<IChannel>> CreateGroupDmAsync
    (
        ICreateGroupDmPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns a list of connection objects for the current user.
    /// </summary>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IConnection>>> GetCurrentUserConnectionsAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the application role connection for the given application for the current user.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of the application administering the connection.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IApplicationRoleConnection>> GetCurrentUserApplicationRoleConnectionAsync
    (
        Snowflake applicationId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Updates the application role connection for the given application for the current user.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of the application administering the connection.</param>
    /// <param name="payload">The new information for this role connection.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly updated connection object.</returns>
    public ValueTask<Result<IApplicationRoleConnection>> UpdateCurrentUserApplicationRoleConnectionAsync
    (
        Snowflake applicationId,
        IUpdateCurrentUserApplicationRoleConnectionPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
