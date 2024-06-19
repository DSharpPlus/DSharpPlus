using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an OAuth2 application.
/// </summary>
public sealed class DiscordApplication : DiscordMessageApplication, IEquatable<DiscordApplication>
{
    /// <summary>
    /// Gets the application's icon.
    /// </summary>
    public override string? Icon
        => !string.IsNullOrWhiteSpace(this.IconHash)
            ? $"https://cdn.discordapp.com/app-icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=1024"
            : null;

    /// <summary>
    /// Gets the application's icon hash.
    /// </summary>
    public string? IconHash { get; internal set; }

    /// <summary>
    /// Gets the application's terms of service URL.
    /// </summary>
    public string? TermsOfServiceUrl { get; internal set; }

    /// <summary>
    /// Gets the application's privacy policy URL.
    /// </summary>
    public string? PrivacyPolicyUrl { get; internal set; }

    /// <summary>
    /// Gets the application's allowed RPC origins.
    /// </summary>
    public IReadOnlyList<string>? RpcOrigins { get; internal set; }

    /// <summary>
    /// Gets the application's flags.
    /// </summary>
    public DiscordApplicationFlags? Flags { get; internal set; }

    /// <summary>
    /// Gets the application's owners.
    /// </summary>
    public IEnumerable<DiscordUser>? Owners { get; internal set; }

    /// <summary>
    /// Gets whether this application's bot user requires code grant.
    /// </summary>
    public bool? RequiresCodeGrant { get; internal set; }

    /// <summary>
    /// Gets whether this bot application is public.
    /// </summary>
    public bool? IsPublic { get; internal set; }

    /// <summary>
    /// Gets the hash of the application's cover image.
    /// </summary>
    public string? CoverImageHash { get; internal set; }

    /// <summary>
    /// Gets this application's cover image URL.
    /// </summary>
    public override string? CoverImageUrl
        => $"https://cdn.discordapp.com/app-icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.CoverImageHash}.png?size=1024";

    /// <summary>
    /// Gets the team which owns this application.
    /// </summary>
    public DiscordTeam? Team { get; internal set; }

    /// <summary>
    /// Public key used to verify http interactions
    /// </summary>
    public string VerifyKey { get; internal set; }

    /// <summary>
    /// Partial user object for the bot user associated with the app.
    /// </summary>
    public DiscordUser? Bot { get; internal set; }

    /// <summary>
    /// Default scopes and permissions for each supported installation context.
    /// </summary>
    public IReadOnlyDictionary<DiscordApplicationIntegrationType, DiscordApplicationIntegrationTypeConfiguration>? IntegrationTypeConfigurations { get; internal set; }

    /// <summary>
    /// Guild associated with the app. For example, a developer support server.
    /// </summary>
    public ulong? GuildId { get; internal set; }

    /// <summary>
    /// Partial object of the associated guild
    /// </summary>
    public DiscordGuild? Guild { get; internal set; }

    /// <summary>
    /// If this app is a game sold on Discord, this field will be the id of the "Game SKU" that is created, if exists
    /// </summary>
    public ulong? PrimarySkuId { get; internal set; }

    /// <summary>
    /// If this app is a game sold on Discord, this field will be the URL slug that links to the store page
    /// </summary>
    public string? Slug { get; internal set; }

    /// <summary>
    /// Approximate count of guilds the app has been added to
    /// </summary>
    public int? ApproximateGuildCount { get; internal set; }

    /// <summary>
    /// Approximate count of users that have installed the app
    /// </summary>
    public int? ApproximateUserInstallCount { get; internal set; }

    /// <summary>
    /// Array of redirect URIs for the app
    /// </summary>
    public string[] RedirectUris { get; internal set; }

    /// <summary>
    /// Interactions endpoint URL for the app
    /// </summary>
    public string? InteractionsEndpointUrl { get; internal set; }

    /// <summary>
    /// Interactions endpoint URL for the app
    /// </summary>
    public string? RoleConnectionsVerificationEndpointUrl { get; internal set; }

    /// <summary>
    /// List of tags describing the content and functionality of the app. Max of 5 tags.
    /// </summary>
    public string[]? Tags { get; internal set; }

    /// <summary>
    /// Settings for the app's default in-app authorization link, if enabled
    /// </summary>
    public DiscordApplicationOAuth2InstallParams? InstallParams { get; internal set; }


    /// <summary>
    /// Default custom authorization URL for the app, if enabled
    /// </summary>
    public string? CustomInstallUrl { get; internal set; }

    private IReadOnlyList<DiscordApplicationAsset>? Assets { get; set; }

    internal DiscordApplication() { }

    internal DiscordApplication(TransportApplication transportApplication)
    {
        this.Id = transportApplication.Id;
        this.Name = transportApplication.Name;
        this.IconHash = transportApplication.IconHash;
        this.Description = transportApplication.Description;
        this.IsPublic = transportApplication.IsPublicBot;
        this.RequiresCodeGrant = transportApplication.BotRequiresCodeGrant;
        this.TermsOfServiceUrl = transportApplication.TermsOfServiceUrl;
        this.PrivacyPolicyUrl = transportApplication.PrivacyPolicyUrl;
        this.RpcOrigins = transportApplication.RpcOrigins != null
            ? new ReadOnlyCollection<string>(transportApplication.RpcOrigins)
            : null;
        this.Flags = transportApplication.Flags;
        this.CoverImageHash = transportApplication.CoverImageHash;
        this.VerifyKey = transportApplication.VerifyKey;

        this.Bot = transportApplication.Bot is null
            ? null
            : new DiscordUser(transportApplication.Bot)
            {
                Discord = this.Discord
            };

        this.GuildId = transportApplication.GuildId;
        this.Guild = transportApplication.Guild;
        if (this.Guild is not null)
        {
            this.Guild.Discord = this.Discord;
        }

        this.PrimarySkuId = transportApplication.PrimarySkuId;
        this.Slug = transportApplication.Slug;
        this.ApproximateGuildCount = transportApplication.ApproximateGuildCount;
        this.ApproximateUserInstallCount = transportApplication.ApproximateUserInstallCount;
        this.RedirectUris = transportApplication.RedirectUris;
        this.InteractionsEndpointUrl = transportApplication.InteractionEndpointUrl;
        this.RoleConnectionsVerificationEndpointUrl = transportApplication.RoleConnectionsVerificationUrl;
        this.Tags = transportApplication.Tags;
        this.InstallParams = transportApplication.InstallParams;
        this.IntegrationTypeConfigurations = transportApplication.IntegrationTypeConfigurations;
        this.CustomInstallUrl = transportApplication.CustomInstallUrl;


        // do team and owners
        // tbh fuck doing this properly
        if (transportApplication.Team == null)
        {
            // singular owner

            this.Owners = new ReadOnlyCollection<DiscordUser>(new[] { new DiscordUser(transportApplication.Owner) });
            this.Team = null;
        }
        else
        {
            // team owner

            this.Team = new DiscordTeam(transportApplication.Team);

            DiscordTeamMember[] members = transportApplication.Team.Members
                .Select(x => new DiscordTeamMember(x) { Team = this.Team, User = new DiscordUser(x.User) })
                .ToArray();

            DiscordUser[] owners = members
                .Where(x => x.MembershipStatus == DiscordTeamMembershipStatus.Accepted)
                .Select(x => x.User)
                .ToArray();

            this.Owners = new ReadOnlyCollection<DiscordUser>(owners);
            this.Team.Owner = owners.FirstOrDefault(x => x.Id == transportApplication.Team.OwnerId);
            this.Team.Members = new ReadOnlyCollection<DiscordTeamMember>(members);
        }
    }


    /// <summary>
    /// Gets the application's cover image URL, in requested format and size.
    /// </summary>
    /// <param name="fmt">Format of the image to get.</param>
    /// <param name="size">Maximum size of the cover image. Must be a power of two, minimum 16, maximum 2048.</param>
    /// <returns>URL of the application's cover image.</returns>
    public string? GetAvatarUrl(ImageFormat fmt, ushort size = 1024)
    {
        if (fmt == ImageFormat.Unknown)
        {
            throw new ArgumentException("You must specify valid image format.", nameof(fmt));
        }

        if (size is < 16 or > 2048)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        double log = Math.Log(size, 2);
        if (log < 4 || log > 11 || log % 1 != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        string formatString = fmt switch
        {
            ImageFormat.Gif => "gif",
            ImageFormat.Jpeg => "jpg",
            ImageFormat.Auto or ImageFormat.Png => "png",
            ImageFormat.WebP => "webp",
            _ => throw new ArgumentOutOfRangeException(nameof(fmt)),
        };

        string ssize = size.ToString(CultureInfo.InvariantCulture);

        if (!string.IsNullOrWhiteSpace(this.CoverImageHash))
        {
            string id = this.Id.ToString(CultureInfo.InvariantCulture);
            return $"https://cdn.discordapp.com/avatars/{id}/{this.CoverImageHash}.{formatString}?size={ssize}";
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Retrieves this application's assets.
    /// </summary>
    /// <param name="updateCache">Whether to always make a REST request and update the cached assets.</param>
    /// <returns>This application's assets.</returns>
    public async Task<IReadOnlyList<DiscordApplicationAsset>> GetAssetsAsync(bool updateCache = false)
    {
        if (updateCache || this.Assets == null)
        {
            this.Assets = await this.Discord.ApiClient.GetApplicationAssetsAsync(this);
        }

        return this.Assets;
    }

    public string GenerateBotOAuth(DiscordPermissions permissions = DiscordPermissions.None)
    {
        permissions &= DiscordPermissions.All;
        // hey look, it's not all annoying and blue :P
        return new QueryUriBuilder("https://discord.com/oauth2/authorize")
            .AddParameter("client_id", this.Id.ToString(CultureInfo.InvariantCulture))
            .AddParameter("scope", "bot")
            .AddParameter("permissions", ((long)permissions).ToString(CultureInfo.InvariantCulture))
            .ToString();
    }

    /// <summary>
    /// Generates a new OAuth2 URI for this application.
    /// </summary>
    /// <param name="redirectUri">Redirect URI - the URI Discord will redirect users to as part of the OAuth flow.
    /// <remarks>
    /// This URI <b>must</b> be already registered as a valid redirect URI for your application on the developer portal.
    /// </remarks>
    /// </param>
    /// <param name="permissions">Permissions for your bot. Only required if the <seealso cref="DiscordOAuthScope.Bot"/> scope is passed.</param>
    /// <param name="scopes">OAuth scopes for your application.</param>
    public string GenerateOAuthUri(string? redirectUri = null, DiscordPermissions? permissions = null,
        params DiscordOAuthScope[] scopes)
    {
        permissions &= DiscordPermissions.All;

        StringBuilder scopeBuilder = new();

        foreach (DiscordOAuthScope v in scopes)
        {
            scopeBuilder.Append(' ').Append(TranslateOAuthScope(v));
        }

        QueryUriBuilder queryBuilder = new QueryUriBuilder("https://discord.com/oauth2/authorize")
            .AddParameter("client_id", this.Id.ToString(CultureInfo.InvariantCulture))
            .AddParameter("scope", scopeBuilder.ToString().Trim());

        if (permissions != null)
        {
            queryBuilder.AddParameter("permissions", ((long)permissions).ToString(CultureInfo.InvariantCulture));
        }

        // response_type=code is always given for /authorize
        if (redirectUri != null)
        {
            queryBuilder.AddParameter("redirect_uri", redirectUri)
                .AddParameter("response_type", "code");
        }

        return queryBuilder.ToString();
    }

    /// <summary>
    /// Checks whether this <see cref="DiscordApplication"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordApplication"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as DiscordApplication);

    /// <summary>
    /// Checks whether this <see cref="DiscordApplication"/> is equal to another <see cref="DiscordApplication"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordApplication"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordApplication"/> is equal to this <see cref="DiscordApplication"/>.</returns>
    public bool Equals(DiscordApplication? e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordApplication"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
    public override int GetHashCode() => this.Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordApplication"/> objects are equal.
    /// </summary>
    /// <param name="right">First application to compare.</param>
    /// <param name="left">Second application to compare.</param>
    /// <returns>Whether the two applications are equal.</returns>
    public static bool operator ==(DiscordApplication right, DiscordApplication left)
    {
        return (right is not null || left is null)
               && (right is null || left is not null)
               && ((right is null && left is null)
                   || right!.Id == left!.Id);
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordApplication"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First application to compare.</param>
    /// <param name="e2">Second application to compare.</param>
    /// <returns>Whether the two applications are not equal.</returns>
    public static bool operator !=(DiscordApplication e1, DiscordApplication e2)
        => !(e1 == e2);

    private static string? TranslateOAuthScope(DiscordOAuthScope scope) => scope switch
    {
        DiscordOAuthScope.Identify => "identify",
        DiscordOAuthScope.Email => "email",
        DiscordOAuthScope.Connections => "connections",
        DiscordOAuthScope.Guilds => "guilds",
        DiscordOAuthScope.GuildsJoin => "guilds.join",
        DiscordOAuthScope.GuildsMembersRead => "guilds.members.read",
        DiscordOAuthScope.GdmJoin => "gdm.join",
        DiscordOAuthScope.Rpc => "rpc",
        DiscordOAuthScope.RpcNotificationsRead => "rpc.notifications.read",
        DiscordOAuthScope.RpcVoiceRead => "rpc.voice.read",
        DiscordOAuthScope.RpcVoiceWrite => "rpc.voice.write",
        DiscordOAuthScope.RpcActivitiesWrite => "rpc.activities.write",
        DiscordOAuthScope.Bot => "bot",
        DiscordOAuthScope.WebhookIncoming => "webhook.incoming",
        DiscordOAuthScope.MessagesRead => "messages.read",
        DiscordOAuthScope.ApplicationsBuildsUpload => "applications.builds.upload",
        DiscordOAuthScope.ApplicationsBuildsRead => "applications.builds.read",
        DiscordOAuthScope.ApplicationsCommands => "applications.commands",
        DiscordOAuthScope.ApplicationsStoreUpdate => "applications.store.update",
        DiscordOAuthScope.ApplicationsEntitlements => "applications.entitlements",
        DiscordOAuthScope.ActivitiesRead => "activities.read",
        DiscordOAuthScope.ActivitiesWrite => "activities.write",
        DiscordOAuthScope.RelationshipsRead => "relationships.read",
        _ => null
    };
    
    //TODO: implement pagination and turn this into IAsyncEnumerable
    /// <summary>
    /// List all stock keeping units belonging to this application
    /// </summary>
    /// <returns></returns>
    public async ValueTask<IReadOnlyList<DiscordStockKeepingUnit>> ListStockKeepingUnitsAsync() 
        => await this.Discord.ApiClient.ListStockKeepingUnitsAsync(this.Id);
}
