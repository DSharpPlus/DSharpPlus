// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;
using DSharpPlus.Entities;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

/// <summary>
/// Provides access to SKUs-related API calls.
/// </summary>
public interface ISkusRestAPI
{
    /// <summary>
    /// Returns all SKUs for a given applications. You will see two SKUs for your premium offering, "because of
    /// how our SKU and subscription systems work" - Discord. <br/>
    /// For integration and testing entitlements you should use the SKU with type
    /// <seealso cref="DiscordSkuType.Subscription"/>.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of the parent application.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IReadOnlyList<ISku>>> ListSkusAsync
    (
        Snowflake applicationId,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
