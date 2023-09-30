// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a premium offering that can be made available to your application's users or guilds.
/// </summary>
public interface ISku
{
    /// <summary>
    /// The identifier of this SKU.
    /// </summary>
    public object Id { get; }

    /// <summary>
    /// The type of this SKU.
    /// </summary>
    public SkuType Type { get; }

    /// <summary>
    /// The snowflake identifier of the parent application.
    /// </summary>
    public Snowflake ApplicationId { get; }

    /// <summary>
    /// The user-facing name of the offering.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A system-generated URL slug based on the SKU's name.
    /// </summary>
    public string Slug { get; }
}
