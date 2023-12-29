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
/// Provides access to entitlements-related API calls.
/// </summary>
public interface IEntitlementsRestAPI
{
    /// <summary>
    /// Returns all entitlements for a given app, according to the query parameters.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of the current application.</param>
    /// <param name="query">Contains filtering and pagination options for this request.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IEntitlement>>> ListEntitlementsAsync
    (
        Snowflake applicationId,
        ListEntitlementsQuery query = default,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Creates a test entitlement to a given SKU for a given guild or user. Discord will act as though that user
    /// or guild has entitlement to your offering. <br/>
    /// After creating a test entitlement, you will need to reload your Discord client. After that, you will
    /// have premium access.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="payload">The target of your test entitlement.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly created test entitlement.</returns>
    public ValueTask<Result<IPartialEntitlement>> CreateTestEntitlementAsync
    (
        Snowflake applicationId,
        ICreateTestEntitlementPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Deletes a currently active test entitlement. Discord will act as though that user or guild no longer
    /// has entitlement to your offering.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="entitlementId">The snowflake identifier of the test entitlement to delete.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result> DeleteTestEntitlementAsync
    (
        Snowflake applicationId,
        Snowflake entitlementId,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
