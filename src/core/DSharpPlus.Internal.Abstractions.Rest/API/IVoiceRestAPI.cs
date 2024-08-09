// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to voice-related API calls.
/// </summary>
public interface IVoiceRestAPI
{
    /// <summary>
    /// Returns an array of voice region objects that can be used when setting a voice or stage channel's rtc region.
    /// </summary>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IVoiceRegion>>> ListVoiceRegionsAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the current user's voice state in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the specified guild.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IVoiceState>> GetCurrentUserVoiceStateAsync
    (
        Snowflake guildId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the specified user's voice state in the specified guild.
    /// </summary>
    /// <param name="guildId">The snowflake identifier specifying the guild.</param>
    /// <param name="userId">The snowflake identifier specifying the user.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IVoiceState>> GetUserVoiceStateAsync
    (
        Snowflake guildId,
        Snowflake userId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the current user's voice state.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild everything takes place in.</param>
    /// <param name="payload">Information on how to update the current voice state.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> ModifyCurrentUserVoiceStateAsync
    (
        Snowflake guildId,
        IModifyCurrentUserVoiceStatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies another user's voice state.
    /// </summary>
    /// <param name="guildId">The snowflake identifier of the guild everything takes place in.</param>
    /// <param name="userId">The snowflake identifier of the user whose voice state to modify.</param>
    /// <param name="payload">Information on how to modify the user's voice state.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> ModifyUserVoiceStateAsync
    (
        Snowflake guildId,
        Snowflake userId,
        IModifyUserVoiceStatePayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
