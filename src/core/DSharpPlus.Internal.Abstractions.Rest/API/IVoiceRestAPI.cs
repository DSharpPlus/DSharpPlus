// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus.Internal.Abstractions.Models;

using Remora.Results;

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
}
