// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an application command, either as a chat input, a message context menu or an user context
/// menu command.
/// </summary>
public interface IApplicationCommand
{
    /// <summary>
    /// The snowflake identifier of this command.
    /// </summary>
    public Snowflake Id { get; }

    /// <summary>
    /// The type of this command, default: <seealso cref="DiscordApplicationCommandType.ChatInput"/>.
    /// </summary>
    public Optional<DiscordApplicationCommandType> Type { get; }

    /// <summary>
    /// The snowflake identifier of the application owning this command.
    /// </summary>
    public Snowflake ApplicationId { get; }

    /// <summary>
    /// If this command is a guild command, the snowflake identifier of its home guild.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// The name of this command, between 1 and 32 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A localization dictionary for <seealso cref="Name"/>, with the keys being locales.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; }

    /// <summary>
    /// If this command is a <seealso cref="DiscordApplicationCommandType.ChatInput"/> command, the
    /// description of this command, between 1 and 100 characters. This is an empty string for all
    /// other command types.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// A localization dictionary for <seealso cref="Description"/>, with the keys being locales.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; }

    /// <summary>
    /// The parameters of this (chat input) command, up to 25.
    /// </summary>
    public Optional<IReadOnlyList<IApplicationCommandOption>> Options { get; }

    /// <summary>
    /// The permissions needed to gain default access to this command.
    /// </summary>
    public DiscordPermissions? DefaultMemberPermissions { get; }

    /// <summary>
    /// Indicates whether this command is available in DMs with the app. This is only applicable
    /// to globally-scoped commands.
    /// </summary>
    public Optional<bool> DmPermission { get; }

    /// <summary>
    /// Indicates whether this command is age-restricted.
    /// </summary>
    public Optional<bool> Nsfw { get; }

    /// <summary>
    /// An autoincrementing version identifier updated during substantial changes.
    /// </summary>
    public Snowflake Version { get; }
}
