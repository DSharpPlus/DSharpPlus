// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents activity data encoded in a message.
/// </summary>
public interface IMessageActivity
{
    /// <summary>
    /// The type of this activity.
    /// </summary>
    public DiscordMessageActivityType Type { get; }

    /// <summary>
    /// The party ID from a rich presence event.
    /// </summary>
    public Optional<string> PartyId { get; }
}
