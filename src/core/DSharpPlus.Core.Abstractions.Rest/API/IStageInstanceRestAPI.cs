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
/// Provides access to stage-instance-related API calls.
/// </summary>
public interface IStageInstanceRestAPI
{
    /// <summary>
    /// Creates a new stage instance associated to a stage channel.
    /// </summary>
    /// <param name="payload">The information to initialize the stage instance with.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created stage instance.</returns>
    public ValueTask<Result<IStageInstance>> CreateStageInstanceAsync
    (
        ICreateStageInstancePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Returns the stage instance associated with the stage channel, if one exists.
    /// </summary>
    /// <param name="channelId">Snowflake identifier of the associated stage channel.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IStageInstance>> GetStageInstanceAsync
    (
        Snowflake channelId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Modifies the given stage instance.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of the parent channel.</param>
    /// <param name="payload">The updated information for this stage instance.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly modified stage instance.</returns>
    public ValueTask<Result<IStageInstance>> ModifyStageInstanceAsync
    (
        Snowflake channelId,
        IModifyStageInstancePayload payload,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the given stage instance.
    /// </summary>
    /// <param name="channelId">The snowflake identifier of its parent channel.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteStageInstanceAsync
    (
        Snowflake channelId,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
