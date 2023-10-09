// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an application, such as a bot, on Discord.
/// </summary>
public interface IApplication : IPartialApplication
{
    /// <inheritdoc cref="IPartialApplication.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialApplication.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialApplication.Icon"/>
    public new string? Icon { get; }

    /// <inheritdoc cref="IPartialApplication.BotPublic"/>
    public new bool BotPublic { get; }

    /// <inheritdoc cref="IPartialApplication.BotRequireCodeGrant"/>
    public new bool BotRequireCodeGrant { get; }

    /// <inheritdoc cref="IPartialApplication.VerifyKey"/>
    public new string VerifyKey { get; }

    /// <inheritdoc cref="IPartialApplication.Team"/>
    public new ITeam? Team { get; }

    // explicit routes for partial application access

    /// <inheritdoc/>
    Optional<Snowflake> IPartialApplication.Id => this.Id;

    /// <inheritdoc/>
    Optional<string> IPartialApplication.Name => this.Name;

    /// <inheritdoc/>
    Optional<string?> IPartialApplication.Icon => this.Icon;

    /// <inheritdoc/>
    Optional<bool> IPartialApplication.BotPublic => this.BotPublic;

    /// <inheritdoc/>
    Optional<bool> IPartialApplication.BotRequireCodeGrant => this.BotRequireCodeGrant;

    /// <inheritdoc/>
    Optional<string> IPartialApplication.VerifyKey => this.VerifyKey;

    /// <inheritdoc/>
    Optional<ITeam?> IPartialApplication.Team => new(this.Team);
}
