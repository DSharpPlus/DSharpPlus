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
/// Provides access to application commands-related API calls.
/// </summary>
// https://discord.com/developers/docs/interactions/application-commands
public interface IApplicationCommandsRestAPI
{
    /// <summary>
    /// Fetches all global application commands for your application.
    /// </summary>
    /// <param name="applicationId">The snowflake identifier of your application.</param>
    /// <param name="withLocalizations">Indicates whether to include full localizations in the returned objects.</param>
    /// <param name="info">Additional instructions regarding this request.</param>
    /// <param name="ct">A cancellation token for this operation.</param>
    /// <returns>An array of application commands.</returns>
    public ValueTask<Result<IReadOnlyList<IApplicationCommand>>> GetGlobalApplicationCommandsAsync
    (
        Snowflake applicationId,
        bool? withLocalizations = null,
        RequestInfo info = default,
        CancellationToken ct = default
    );
}
