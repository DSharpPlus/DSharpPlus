// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Internal.Abstractions.Rest.Payloads;
using DSharpPlus.Results;

namespace DSharpPlus.Internal.Abstractions.Rest.API;

/// <summary>
/// Provides access to application-related rest API calls.
/// </summary>
public interface IApplicationRestAPI
{
    /// <summary>
    /// Returns the application object associated with the requesting bot user.
    /// </summary>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    public ValueTask<Result<IApplication>> GetCurrentApplicationAsync
    (
        RequestInfo info = default,
        CancellationToken ct = default
    );

    /// <summary>
    /// Edits the application associated with the requesting bot user.
    /// </summary>
    /// <param name="payload">A payload object containing properties to update.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>The updated application object.</returns>
    public ValueTask<Result<IApplication>> EditCurrentApplicationAsync
    (
        IEditCurrentApplicationPayload payload,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
