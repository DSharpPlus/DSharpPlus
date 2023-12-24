// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to invite-related API calls.
/// </summary>
public interface IInviteRestAPI
{
    /// <summary>
    /// Returns the queried invite.
    /// </summary>
    /// <param name="inviteCode">Invite code identifying this invite.</param>
    /// <param name="withCounts">Indicates whether the invite should contain approximate member counts.</param>
    /// <param name="withExpiration">Indicates whether the invite should contain the expiration time.</param>
    /// <param name="guildScheduledEventId">The scheduled event to include with the invite.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IInvite>> GetInviteAsync
    (
        string inviteCode,
        bool? withCounts = null,
        bool? withExpiration = null,
        Snowflake? guildScheduledEventId = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes the given invite.
    /// </summary>
    /// <param name="inviteCode">The code identifying the invite.</param>
    /// <param name="reason">An optional audit log reason.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The deleted invite object.</returns>
    public ValueTask<Result<IInvite>> DeleteInviteAsync
    (
        string inviteCode,
        string? reason = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
