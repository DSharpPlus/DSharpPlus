// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents metadata for auto moderation actions of type 
/// <seealso cref="DiscordAutoModerationActionType.BlockMessage"/>.
/// </summary>
public interface IBlockMessageActionMetadata : IAutoModerationActionMetadata
{
    /// <summary>
    /// An explanation that will be shown to members whenever their message is blocked, up to 150 characters.
    /// </summary>
    public Optional<string> CustomMessage { get; }
}
