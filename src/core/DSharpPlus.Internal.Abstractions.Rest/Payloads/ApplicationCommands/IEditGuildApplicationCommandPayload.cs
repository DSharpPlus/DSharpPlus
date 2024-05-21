// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Internal.Abstractions.Models;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>PATCH /applications/:application-id/guilds/:guild-id/commands/:command-id</c>.
/// </summary>
public interface IEditGuildApplicationCommandPayload
{
    /// <summary>
    /// The name of this command, between 1 and 32 characters.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// A localization dictionary for the <see cref="Name"/> field. Values follow the same restrictions.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; }

    /// <summary>
    /// The description for this chat input command, between 1 and 100 characters.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// A localization dictionary for the <see cref="Description"/> field. Values follow the same restrictions.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; }

    /// <summary>
    /// Up to 25 parameters for this command, or its subcommands.
    /// </summary>
    public Optional<IReadOnlyList<IApplicationCommandOption>> Options { get; }

    /// <summary>
    /// The default permissions needed to see this command.
    /// </summary>
    public Optional<DiscordPermissions?> DefaultMemberPermissions { get; }

    /// <summary>
    /// The type of this command.
    /// </summary>
    public Optional<DiscordApplicationCommandType> Type { get; }

    /// <summary>
    /// Indicates whether this command is age-restricted.
    /// </summary>
    public Optional<bool> Nsfw { get; }
}
