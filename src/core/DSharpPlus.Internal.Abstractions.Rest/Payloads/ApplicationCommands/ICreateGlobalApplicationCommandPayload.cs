// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Rest.Payloads;

/// <summary>
/// Represents a payload to <c>POST /applications/:application-id/commands</c>.
/// </summary>
public interface ICreateGlobalApplicationCommandPayload
{
    /// <summary>
    /// The name of this command, between 1 and 32 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A localization dictionary for the <seealso cref="Name"/> field. Values follow the same restrictions.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; }

    /// <summary>
    /// The description for this chat input command, between 1 and 100 characters.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// A localization dictionary for the <seealso cref="Description"/> field. Values follow the same restrictions.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; }

    /// <summary>
    /// The default permissions needed to see this command.
    /// </summary>
    public Optional<DiscordPermissions?> DefaultMemberPermissions { get; }

    /// <summary>
    /// Indicates whether this command is available in DMs with this app.
    /// </summary>
    public Optional<bool?> DmPermission { get; }

    /// <summary>
    /// The type of this command.
    /// </summary>
    public Optional<DiscordApplicationCommandType> Type { get; }
    
    /// <summary>
    /// Indicates whether this command is age-restricted.
    /// </summary>
    public Optional<bool> Nsfw { get; }
}
