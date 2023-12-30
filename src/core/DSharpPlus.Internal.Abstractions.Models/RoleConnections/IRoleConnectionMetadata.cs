// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Contains role connection metadata for an application.
/// </summary>
public interface IRoleConnectionMetadata
{
    /// <summary>
    /// The type and comparison type of the metadata value.
    /// </summary>
    public DiscordRoleConnectionMetadataType Type { get; }

    /// <summary>
    /// The dictionary key for the metadata field, between 1 and 50 characters.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The name of this metadata field, between 1 and 100 characters.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// A localization dictionary for <seealso cref="Name"/>, with the keys being locales.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> NameLocalizations { get; }

    /// <summary>
    /// A description for this metadata field, between 1 and 200 characters.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// A localization dictionary for <seealso cref="Description"/>, with the keys being locales.
    /// </summary>
    public Optional<IReadOnlyDictionary<string, string>?> DescriptionLocalizations { get; }
}
