// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an action which will execute whenever a rule is triggered.
/// </summary>
public interface IAutoModerationAction
{
    /// <summary>
    /// The type of this action.
    /// </summary>
    public DiscordAutoModerationActionType Type { get; }

    /// <summary>
    /// Additional metadata for executing this action.
    /// </summary>
    public Optional<IAutoModerationActionMetadata> Metadata { get; }
}
