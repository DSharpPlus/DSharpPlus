// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Rest.Ratelimiting;

/// <summary>
/// Specifies the top-level resources for requests.
/// </summary>
internal enum TopLevelResource
{
    Channel,
    Guild,
    Webhook,
    Other
}
