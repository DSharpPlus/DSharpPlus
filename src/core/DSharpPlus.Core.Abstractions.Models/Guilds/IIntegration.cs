// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using Remora.Rest.Core;

namespace DSharpPlus.Core.Abstractions.Models;

/// <summary>
/// Represents an integration between an external service and a guild.
/// </summary>
public interface IIntegration : IPartialIntegration
{
    /// <inheritdoc cref="IPartialIntegration.Id"/>
    public new Snowflake Id { get; }

    /// <inheritdoc cref="IPartialIntegration.Name"/>
    public new string Name { get; }

    /// <inheritdoc cref="IPartialIntegration.Type"/>
    public new string Type { get; }

    /// <inheritdoc cref="IPartialIntegration.Enabled"/>
    public new bool Enabled { get; }

    /// <inheritdoc cref="IPartialIntegration.Account"/>
    public new IIntegrationAccount Account { get; }

    // partial access routes

    /// <inheritdoc/>
    Optional<Snowflake> IPartialIntegration.Id => this.Id;

    /// <inheritdoc/>
    Optional<string> IPartialIntegration.Name => this.Name;

    /// <inheritdoc/>
    Optional<string> IPartialIntegration.Type => this.Type;

    /// <inheritdoc/>
    Optional<bool> IPartialIntegration.Enabled => this.Enabled;

    /// <inheritdoc/>
    Optional<IIntegrationAccount> IPartialIntegration.Account => new(this.Account);
}
