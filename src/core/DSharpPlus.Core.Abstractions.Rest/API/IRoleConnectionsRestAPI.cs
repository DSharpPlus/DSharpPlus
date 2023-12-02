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
/// Provides access to managing role connections via the API.
/// </summary>
public interface IRoleConnectionsRestAPI
{
    /// <summary>
    /// Returns the role connection metadata records for the given application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<IRoleConnectionMetadata>>> GetRoleConnectionMetadataRecordsAsync
    (
        Snowflake applicationId,
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Updates the role connection metadata records for the given application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="payload">The new metadata records for this application.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The newly updated metadata records.</returns>
    public ValueTask<Result<IReadOnlyList<IRoleConnectionMetadata>>> UpdateRoleConnectionMetadataRecordsAsync
    (
        Snowflake applicationId,
        IReadOnlyList<IRoleConnectionMetadata> payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
