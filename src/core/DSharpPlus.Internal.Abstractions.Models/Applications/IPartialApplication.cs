// This Source Code form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

using DSharpPlus.Entities;

namespace DSharpPlus.Internal.Abstractions.Models;

/// <summary>
/// Represents a partially populated application object.
/// </summary>
public interface IPartialApplication
{
    /// <summary>
    /// The snowflake identifier of this application.
    /// </summary>
    public Optional<Snowflake> Id { get; }

    /// <summary>
    /// The name of this application.
    /// </summary>
    public Optional<string> Name { get; }

    /// <summary>
    /// The icon hash of this application.
    /// </summary>
    public Optional<string?> Icon { get; }

    /// <summary>
    /// The description of this application. This doubles as the associated bot's about me section.
    /// </summary>
    public Optional<string> Description { get; }

    /// <summary>
    /// An array of RPC origin urls, if RPC is enabled.
    /// </summary>
    public Optional<IReadOnlyList<string>> RpcOrigins { get; }

    /// <summary>
    /// Indicates whether this application's bot is publicly invitable.
    /// </summary>
    public Optional<bool> BotPublic { get; }

    /// <summary>
    /// Indicates whether the bot will require completion of the OAuth2 code grant flow to join.
    /// </summary>
    public Optional<bool> BotRequireCodeGrant { get; }

    /// <summary>
    /// A partial user object for the bot user associated with this app.
    /// </summary>
    public Optional<IPartialUser> Bot { get; }

    /// <summary>
    /// The URL to this application's terms of service.
    /// </summary>
    public Optional<string> TermsOfServiceUrl { get; }

    /// <summary>
    /// The URL to this application's privacy policy.
    /// </summary>
    public Optional<string> PrivacyPolicyUrl { get; }

    /// <summary>
    /// A partial user object containing information on the owner of the application.
    /// </summary>
    public Optional<IPartialUser> Owner { get; }

    /// <summary>
    /// The verification key for interactions and GameSDK functions.
    /// </summary>
    public Optional<string> VerifyKey { get; }

    /// <summary>
    /// The team owning this application.
    /// </summary>
    public Optional<ITeam?> Team { get; }

    /// <summary>
    /// The snowflake identifier of the guild associated with this app, for instance, its support server.
    /// </summary>
    public Optional<Snowflake> GuildId { get; }

    /// <summary>
    /// A partial object of the associated guild.
    /// </summary>
    public Optional<IPartialGuild> Guild { get; }

    /// <summary>
    /// If this application is a game sold on discord, this is the snowflake identifier of the
    /// game SKU created, if it exists.
    /// </summary>
    public Optional<Snowflake> PrimarySkuId { get; }

    /// <summary>
    /// If this application is a game sold on discord, this is the URL slug that links to the store page.
    /// </summary>
    public Optional<string> Slug { get; }

    /// <summary>
    /// The image hash of this application's default rich presence invite cover image.
    /// </summary>
    public Optional<string> CoverImage { get; }

    /// <summary>
    /// The public flags for this application.
    /// </summary>
    public Optional<DiscordApplicationFlags> Flags { get; }

    /// <summary>
    /// An approximate count of this application's guild membership.
    /// </summary>
    public Optional<int> ApproximateGuildCount { get; }

    /// <summary>
    /// An array of redirect URIs for this application.
    /// </summary>
    public Optional<IReadOnlyList<string>> RedirectUris { get; }

    /// <summary>
    /// The interactions endpoint url for this app, if it uses HTTP interactions.
    /// </summary>
    public Optional<string> InteractionsEndpointUrl { get; }

    /// <summary>
    /// This application's role connection verification entry point; which, when configured, will render
    /// the application as a verification method in the guild role verification configuration.
    /// </summary>
    public Optional<string> RoleConnectionsVerificationUrl { get; }

    /// <summary>
    /// Up to five tags describing content and functionality of the application.
    /// </summary>
    public Optional<IReadOnlyList<string>> Tags { get; }

    /// <summary>
    /// The installation parameters for this app.
    /// </summary>
    public Optional<IInstallParameters> InstallParams { get; }

    /// <summary>
    /// The default scopes and permissions for each supported installation context.
    /// </summary>
    public Optional<IReadOnlyDictionary<DiscordApplicationIntegrationType, IApplicationIntegrationTypeConfiguration>> IntegrationTypesConfig { get; }

    /// <summary>
    /// The default custom authorization link for this application, if enabled.
    /// </summary>
    public Optional<string> CustomInstallUrl { get; }

    /// <summary>
    /// An approximate to the amount of users who installed this app.
    /// </summary>
    public Optional<int> ApproximateUserInstallCount { get; }
}
