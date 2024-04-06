// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Specifies the default scopes and permissions for a given application.
/// </summary>
public interface IApplicationIntegrationTypeConfiguration
{
    /// <summary>
    /// The installation parameters for each context's default in-app installation link.
    /// </summary>
    public Optional<IInstallParameters> Oauth2InstallParams { get; }
}
