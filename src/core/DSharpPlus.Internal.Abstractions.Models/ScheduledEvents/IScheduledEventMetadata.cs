// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents metadata for a scheduled event.
/// </summary>
public interface IScheduledEventMetadata
{
    /// <summary>
    /// The location of the event, up to 100 characters.
    /// </summary>
    public Optional<string> Location { get; }
}
