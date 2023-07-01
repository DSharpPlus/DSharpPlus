// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Entities;

/// <summary>
/// Specifies the different entity types for events; influencing what fields are present on the parent
/// object.
/// </summary>
public enum DiscordScheduledEventType
{
    StageInstance = 1,
    Voice
}
