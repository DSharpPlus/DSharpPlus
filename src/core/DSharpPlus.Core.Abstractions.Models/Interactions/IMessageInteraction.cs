// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using DSharpPlus.Entities;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents metadata on a message when the message is an original response to an interaction.
/// </summary>
public interface IMessageInteraction
{
    /// <summary>
    /// The snowflake identifier of the interaction.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The type of this interaction.
    /// </summary>
    public DiscordInteractionType Type { get; }

    /// <summary>
    /// The name of the application command invoked, including subcommands and subcommand groups.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The user who invoked this interaction.
    /// </summary>
    public IUser User { get; }

    /// <summary>
    /// The guild member who invoked this interaction.
    /// </summary>
    public Optional<IPartialGuildMember> Member { get; }
}
