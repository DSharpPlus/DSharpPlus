// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents metadata for auto moderation actions of type
/// <seealso cref="DiscordAutoModerationActionType.Timeout"/>.
/// </summary>
public interface ITimeoutActionMetadata : IAutoModerationActionMetadata
{
    /// <summary>
    /// The timeout duration in seconds, up to 2419200 seconds, or 28 days.
    /// </summary>
    public int DurationSeconds { get; }
}
