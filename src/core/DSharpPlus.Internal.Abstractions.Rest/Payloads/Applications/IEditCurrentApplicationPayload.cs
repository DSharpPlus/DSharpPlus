// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Internal.Abstractions.Models;
using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /applications/@me</c>.
/// </summary>
public interface IEditCurrentApplicationPayload
{
    /// <summary>
    /// The default custom authorization URL for this application, if the feature is enabled.
    /// </summary>
    public Optional<string> CustomInstallUrl { get; }

    /// <summary>
    /// The description of this application.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// The role connection verification URL for this application.
    /// </summary>
    public Optional<string> RoleConnectionsVerificationUrl { get; }

    /// <summary>
    /// Settings for this application's default in-app authorization link, if enabled.
    /// </summary>
    public Optional<IInstallParameters> InstallParams { get; }

    /// <summary>
    /// The public flags for this application.
    /// </summary>
    public Optional<DiscordApplicationFlags> Flags { get; }

    /// <summary>
    /// The icon for this application.
    /// </summary>
    public Optional<ImageData?> Icon { get; }

    /// <summary>
    /// The default rich presence invite cover image for this application.
    /// </summary>
    public Optional<ImageData?> CoverImage { get; }

    /// <summary>
    /// The interactions endpoint url for this application.
    /// </summary>
    public Optional<string> InteractionsEndpointUrl { get; }

    /// <summary>
    /// A list of tags describing the content and functionality of this application, with a maximum of
    /// five tags and a maximum of 20 characters per tag.
    /// </summary>
    public Optional<IReadOnlyList<string>> Tags { get; }
}
