// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;

using Remora.Results;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to guild-template-related API calls.
/// </summary>
public interface IGuildTemplateRestAPI
{
    /// <summary>
    /// Fetches the guild template object corresponding to the given template code.
    /// </summary>
    /// <param name="templateCode">The template code in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<ITemplate>> GetGuildTemplateAsync
    (
        string templateCode,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new guild from the given guild template.
    /// </summary>
    /// <remarks>
    /// This endpoint can only be used by bots in less than 10 guilds.
    /// </remarks>
    /// <param name="templateCode">A template code to create the guild from.</param>
    /// <param name="payload">Additional information to initialize this guild with.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created guild.</returns>
    public ValueTask<Result<IGuild>> CreateGuildFromGuildTemplateAsync
    (
        string templateCode,
        ICreateGuildFromGuildTemplatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns all guild templates associated with this guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<ITemplate>>> GetGuildTemplatesAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a new guild template from the given guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="payload">The information to initialize this request with.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created guild template.</returns>
    public ValueTask<Result<ITemplate>> CreateGuildTemplateAsync
    (
        Snowflake guildId,
        ICreateGuildTemplatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Syncs the given template to the given guild's current state.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="templateCode">The code of the template in question.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly modified guild template.</returns>
    public ValueTask<Result<ITemplate>> SyncGuildTemplateAsync
    (
        Snowflake guildId,
        string templateCode,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the given guild template.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="templateCode">Template code of the template in question.</param>
    /// <param name="payload">The new contents of this template.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly modified guild template.</returns>
    public ValueTask<Result<ITemplate>> ModifyGuildTemplateAsync
    (
        Snowflake guildId,
        string templateCode,
        IModifyGuildTemplatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the given guild template.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild in question.</param>
    /// <param name="templateCode">The code of the template to be deleted.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The deleted guild template.</returns>
    public ValueTask<Result<ITemplate>> DeleteGuildTemplateAsync
    (
        Snowflake guildId,
        string templateCode,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
