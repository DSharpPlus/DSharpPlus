// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Contains the data for an user's avatar decoration.
/// </summary>
public interface IAvatarDecorationData
{
    /// <summary>
    /// The hash of the avatar decoration.
    /// </summary>
    public string Asset { get; }

    /// <summary>
    /// The snowflake identifier of the associated SKU.
    /// </summary>
    public Snowflake SkuId { get; }
}
