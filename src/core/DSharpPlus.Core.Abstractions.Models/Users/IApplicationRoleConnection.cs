// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a role connection object that an application has attached to a user.
/// </summary>
public interface IApplicationRoleConnection
{
    /// <summary>
    /// The vanity name of the platform the application has connected, up to 50 characters.
    /// </summary>
    public string? PlatformName { get; }

    /// <summary>
    /// The username of this user on the platform the application has connected, up to 100 characters.
    /// </summary>
    public string? PlatformUsername { get; }

    /// <summary>
    /// The metadata keys and values for this user on the given platform.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
