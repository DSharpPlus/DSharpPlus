// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Contains additional information about integration accounts.
/// </summary>
public interface IIntegrationAccount
{
    /// <summary>
    /// The ID of this account.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The name of this account.
    /// </summary>
    public string Name { get; }
}
