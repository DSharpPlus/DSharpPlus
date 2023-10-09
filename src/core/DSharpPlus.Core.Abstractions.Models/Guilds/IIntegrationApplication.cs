// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents application-specific metadata for an integration.
/// </summary>
public interface IIntegrationApplication
{
    /// <summary>
    /// The snowflake identifier of this application.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The name of this application.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The icon hash of this application.
    /// </summary>
    public string? Icon { get; }

    /// <summary>
    /// The description of this application.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The bot user associated with this application.
    /// </summary>
    public Optional<IUser> Bot { get; }
}
