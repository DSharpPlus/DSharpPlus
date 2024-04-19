// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains query parameters for application command endpoints that can include localizations
/// along with the command object.
/// </summary>
public readonly record struct LocalizationQuery
{
    /// <summary>
    /// Indicates whether to include command localizations with the command object.
    /// </summary>
    public bool? WithLocalizations { get; init; }
}
