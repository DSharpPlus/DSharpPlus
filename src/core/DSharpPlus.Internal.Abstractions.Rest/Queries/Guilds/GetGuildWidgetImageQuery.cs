// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Rest.Queries;

/// <summary>
/// Contains the request parameters to <c>IGuildRestAPI.GetGuildWidgetImageAsync</c>.
/// </summary>
public readonly record struct GetGuildWidgetImageQuery
{
    /// <summary>
    /// The style of the widget image, one of 'shield' or 'banner1' through 'banner4'.
    /// </summary>
    public string? Style { get; init; }
}
