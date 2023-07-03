// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents a guild template that, when used, creates a guild with default settings taken from
/// a snapshot of an existing guild.
/// </summary>
public interface ITemplate
{
    /// <summary>
    /// The unique template code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The name of this template.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description for this template.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The amount of times this template has been used.
    /// </summary>
    public int UsageCount { get; }

    /// <summary>
    /// The snowflake identifier of the user who created this template.
    /// </summary>
    public Snowflake CreatorId { get; }

    /// <summary>
    /// The user who created this template.
    /// </summary>
    public IUser Creator { get; }

    /// <summary>
    /// Indicates when this template was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Indicates when this template was last synced to the source guild.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; }

    /// <summary>
    /// The snowflake identifier of the guild this template is based on.
    /// </summary>
    public Snowflake SourceGuildId { get; }

    /// <summary>
    /// The guild snapshot this template contains.
    /// </summary>
    public IPartialGuild SerializedSourceGuild { get; }

    /// <summary>
    /// Indicates whether this template has unsynced changes.
    /// </summary>
    public bool? IsDirty { get; }
}
