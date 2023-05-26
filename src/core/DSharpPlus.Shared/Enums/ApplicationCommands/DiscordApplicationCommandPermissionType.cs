// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Lists the different targets for an application command permission override.
/// </summary>
public enum DiscordApplicationCommandPermissionType
{
    Role = 1,
    User,
    Channel
}
