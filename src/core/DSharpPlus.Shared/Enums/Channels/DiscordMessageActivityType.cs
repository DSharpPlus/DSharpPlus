// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Enumerates valid activity kinds to be encoded in a message.
/// </summary>
public enum DiscordMessageActivityType
{
    Join = 1,
    Spectate,
    Listen,
    JoinRequest = 5
}
