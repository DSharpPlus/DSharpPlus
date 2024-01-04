// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Net.Http;

namespace DSharpPlus.Internal.Abstractions.Rest;

/// <summary>
/// Represents a basic rest request to Discord.
/// </summary>
public interface IRestRequest
{
    /// <summary>
    /// Gets an attached RequestInfo.
    /// </summary>
    public RequestInfo Info { get; }

    /// <summary>
    /// Builds this request into a request message.
    /// </summary>
    public HttpRequestMessage Build();
}
