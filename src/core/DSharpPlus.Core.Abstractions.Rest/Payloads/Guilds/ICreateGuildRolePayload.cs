// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /guilds/:guild-id/roles</c>
/// </summary>
public interface ICreateGuildRolePayload
{
    /// <summary>
    /// The name of the to-be-created role.
    /// </summary>
    public Optional<string> Name { get; init; }

    /// <summary>
    /// Permissions for this role. Defaults to the @everyone permissions.
    /// </summary>
    public Optional<DiscordPermissions> Permissions { get; init; }

    /// <summary>
    /// RGB color value for this role.
    /// </summary>
    public Optional<int> Color { get; init; }

    /// <summary>
    /// Whether the role should be hoisted in the sidebar. Defaults to <see langword="false"/>.
    /// </summary>
    public Optional<bool> Hoist { get; init; }

    /// <summary>
    /// The role's icon image, if it is a custom icon.
    /// </summary>
    public Optional<ImageData?> Icon { get; init; }

    /// <summary>
    /// The role's unicode emoji as role icon, if applicable.
    /// </summary>
    public Optional<string?> UnicodeEmoji { get; init; }

    /// <summary>
    /// Indicates whether the role should be mentionable by everyone.
    /// </summary>
    public Optional<bool> Mentionable { get; init; }
}
