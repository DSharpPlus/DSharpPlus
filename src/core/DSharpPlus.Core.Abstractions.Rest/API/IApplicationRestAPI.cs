// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Core.Abstractions.Models;

using Remora.Results;

namespace DSharpPlus.Core.Abstractions.Rest.API;

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
}
