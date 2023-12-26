// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to entitlements-related API calls.
/// </summary>
public interface IEntitlementsRestAPI
{
    /// <summary>
    /// Returns all entitlements for a given app, according to the query parameters.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of the current application.</param>
    /// <param name="userId">The snowflake identifier of the user to look up entitlements for.</param>
    /// <param name="skuIds">A comma-delimited set of snowflakes of SKUs to check entitlements for.</param>
    /// <param name="before">Specifies an entitlement to retrieve entitlements before of, for pagination.</param>
    /// <param name="after">Specifies an entitlement to retrieve entitlements after of, for pagination.</param>
    /// <param name="limit">The maximum amount of entitlements to return, up to 100.</param>
    /// <param name="guildId">The snowflake identifier of the guild to look up entitlements for.</param>
    /// <param name="excludeEnded">Specifies whether or not to include ended entitlements.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IEntitlement>>> ListEntitlementsAsync
    (
        Snowflake applicationId,
        Snowflake? userId = null,
        string? skuIds = null,
        Snowflake? before = null,
        Snowflake? after = null,
        int? limit = null,
        Snowflake? guildId = null,
        bool? excludeEnded = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
