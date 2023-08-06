// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

using Remora.Rest.Core;

using DSharpPlus.Core.Abstractions.Models;

namespace DSharpPlus.Core.Models;

/// <inheritdoc cref="IPartialApplication" />
public sealed record PartialApplication : IPartialApplication
{
    /// <inheritdoc/>
    public Optional<Snowflake> Id { get; init; }

    /// <inheritdoc/>
    public Optional<string> Name { get; init; }

    /// <inheritdoc/>
    public Optional<string?> Icon { get; init; }

    /// <inheritdoc/>
    public Optional<string> Description { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> RpcOrigins { get; init; }

    /// <inheritdoc/>
    public Optional<bool> BotPublic { get; init; }

    /// <inheritdoc/>
    public Optional<bool> BotRequireCodeGrant { get; init; }

    /// <inheritdoc/>
    public Optional<string> TermsOfServiceUrl { get; init; }

    /// <inheritdoc/>
    public Optional<string> PrivacyPolicyUrl { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialUser> Owner { get; init; }

    /// <inheritdoc/>
    public Optional<string> VerifyKey { get; init; }

    /// <inheritdoc/>
    public Optional<ITeam?> Team { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> GuildId { get; init; }

    /// <inheritdoc/>
    public Optional<IPartialGuild> Guild { get; init; }

    /// <inheritdoc/>
    public Optional<Snowflake> PrimarySkuId { get; init; }

    /// <inheritdoc/>
    public Optional<string> Slug { get; init; }

    /// <inheritdoc/>
    public Optional<string> CoverImage { get; init; }

    /// <inheritdoc/>
    public Optional<DiscordApplicationFlags> Flags { get; init; }

    /// <inheritdoc/>
    public Optional<int> ApproximateGuildCount { get; init; }

    /// <inheritdoc/>
    public Optional<IReadOnlyList<string>> Tags { get; init; }

    /// <inheritdoc/>
    public Optional<IInstallParameters> InstallParams { get; init; }

    /// <inheritdoc/>
    public Optional<string> CustomInstallUrl { get; init; }

    /// <inheritdoc/>
    public Optional<string> RoleConnectionsVerificationUrl { get; init; }
}