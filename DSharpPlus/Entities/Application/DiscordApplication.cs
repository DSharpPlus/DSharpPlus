using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents an OAuth2 application.
/// </summary>
public sealed class DiscordApplication : DiscordMessageApplication, IEquatable<DiscordApplication>
{
    /// <summary>
    /// Gets the application's summary.
    /// </summary>
    public string? Summary { get; internal set; }

    /// <summary>
    /// Gets the application's icon.
    /// </summary>
    public override string? Icon
        => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/app-icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=1024" : null;

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
    public ApplicationFlags? Flags { get; internal set; }

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

    private IReadOnlyList<DiscordApplicationAsset>? Assets { get; set; }

    internal DiscordApplication() { }

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

        if (size < 16 || size > 2048)
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

    public string GenerateBotOAuth(Permissions permissions = Permissions.None)
    {
        permissions &= PermissionMethods.FULL_PERMS;
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
    /// <param name="permissions">Permissions for your bot. Only required if the <seealso cref="OAuthScope.Bot"/> scope is passed.</param>
    /// <param name="scopes">OAuth scopes for your application.</param>
    public string GenerateOAuthUri(string? redirectUri = null, Permissions? permissions = null, params OAuthScope[] scopes)
    {
        permissions &= PermissionMethods.FULL_PERMS;

        StringBuilder scopeBuilder = new();

        foreach (OAuthScope v in scopes)
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
    public override bool Equals(object? obj) => this.Equals(obj as DiscordApplication);

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

    private static string? TranslateOAuthScope(OAuthScope scope)
    {
        return scope switch
        {
            OAuthScope.Identify => "identify",
            OAuthScope.Email => "email",
            OAuthScope.Connections => "connections",
            OAuthScope.Guilds => "guilds",
            OAuthScope.GuildsJoin => "guilds.join",
            OAuthScope.GuildsMembersRead => "guilds.members.read",
            OAuthScope.GdmJoin => "gdm.join",
            OAuthScope.Rpc => "rpc",
            OAuthScope.RpcNotificationsRead => "rpc.notifications.read",
            OAuthScope.RpcVoiceRead => "rpc.voice.read",
            OAuthScope.RpcVoiceWrite => "rpc.voice.write",
            OAuthScope.RpcActivitiesWrite => "rpc.activities.write",
            OAuthScope.Bot => "bot",
            OAuthScope.WebhookIncoming => "webhook.incoming",
            OAuthScope.MessagesRead => "messages.read",
            OAuthScope.ApplicationsBuildsUpload => "applications.builds.upload",
            OAuthScope.ApplicationsBuildsRead => "applications.builds.read",
            OAuthScope.ApplicationsCommands => "applications.commands",
            OAuthScope.ApplicationsStoreUpdate => "applications.store.update",
            OAuthScope.ApplicationsEntitlements => "applications.entitlements",
            OAuthScope.ActivitiesRead => "activities.read",
            OAuthScope.ActivitiesWrite => "activities.write",
            OAuthScope.RelationshipsRead => "relationships.read",
            _ => null
        };
    }
}

public abstract class DiscordAsset
{
    /// <summary>
    /// Gets the ID of this asset.
    /// </summary>
    public virtual string Id { get; set; } = default!;

    /// <summary>
    /// Gets the URL of this asset.
    /// </summary>
    public abstract Uri Url { get; }
}

/// <summary>
/// Represents an asset for an OAuth2 application.
/// </summary>
public sealed class DiscordApplicationAsset : DiscordAsset, IEquatable<DiscordApplicationAsset>
{
    /// <summary>
    /// Gets the Discord client instance for this asset.
    /// </summary>
    internal BaseDiscordClient? Discord { get; set; }

    /// <summary>
    /// Gets the asset's name.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; } = default!;

    /// <summary>
    /// Gets the asset's type.
    /// </summary>
    [JsonProperty("type")]
    public ApplicationAssetType Type { get; internal set; }

    /// <summary>
    /// Gets the application this asset belongs to.
    /// </summary>
    public DiscordApplication Application { get; internal set; } = default!;

    /// <summary>
    /// Gets the Url of this asset.
    /// </summary>
    public override Uri Url
        => new($"https://cdn.discordapp.com/app-assets/{this.Application.Id.ToString(CultureInfo.InvariantCulture)}/{this.Id}.png");

    internal DiscordApplicationAsset() { }

    internal DiscordApplicationAsset(DiscordApplication app) => this.Discord = app.Discord;

    /// <summary>
    /// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
    public override bool Equals(object? obj) => this.Equals(obj as DiscordApplicationAsset);

    /// <summary>
    /// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another <see cref="DiscordApplicationAsset"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordApplicationAsset"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordApplicationAsset"/> is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
    public bool Equals(DiscordApplicationAsset? e) => e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordApplication"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
    public override int GetHashCode() => this.Id.GetHashCode();

    /// <summary>
    /// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are equal.
    /// </summary>
    /// <param name="right">First application asset to compare.</param>
    /// <param name="left">Second application asset to compare.</param>
    /// <returns>Whether the two application assets not equal.</returns>
    public static bool operator ==(DiscordApplicationAsset right, DiscordApplicationAsset left)
    {
        return (right is not null || left is null)
            && (right is null || left is not null)
            && ((right is null && left is null)
                || right!.Id == left!.Id);
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First application asset to compare.</param>
    /// <param name="e2">Second application asset to compare.</param>
    /// <returns>Whether the two application assets are not equal.</returns>
    public static bool operator !=(DiscordApplicationAsset e1, DiscordApplicationAsset e2)
        => !(e1 == e2);
}

public sealed class DiscordSpotifyAsset : DiscordAsset
{
    /// <summary>
    /// Gets the URL of this asset.
    /// </summary>
    public override Uri Url
        => this._url;

    private readonly Uri _url;

    public DiscordSpotifyAsset()
    {
        string[] ids = this.Id.Split(':');
        string id = ids[1];

        this._url = new Uri($"https://i.scdn.co/image/{id}");
    }
}

/// <summary>
/// Determines the type of the asset attached to the application.
/// </summary>
public enum ApplicationAssetType : int
{
    /// <summary>
    /// Unknown type. This indicates something went terribly wrong.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// This asset can be used as small image for rich presences.
    /// </summary>
    SmallImage = 1,

    /// <summary>
    /// This asset can be used as large image for rich presences.
    /// </summary>
    LargeImage = 2
}

/// <summary>
/// Represents the possible OAuth scopes for application authorization.
/// </summary>
public enum OAuthScope : int
{
    /// <summary>
    /// Allows <c>/users/@me</c> without <c>email</c>.
    /// </summary>
    Identify,

    /// <summary>
    /// Enables <c>/users/@me</c> to return <c>email</c>.
    /// </summary>
    Email,

    /// <summary>
    /// Allows <c>/users/@me/connections</c> to return linked third-party accounts.
    /// </summary>
    Connections,

    /// <summary>
    /// Allows <c>/users/@me/guilds</c> to return basic information about all of a user's guilds.
    /// </summary>
    Guilds,

    /// <summary>
    /// Allows <c>/guilds/{guild.id}/members/{user.id}</c> to be used for joining users into a guild.
    /// </summary>
    GuildsJoin,

    /// <summary>
    /// Allows <c>/users/@me/guilds/{guild.id}/members</c> to return a user's member information in a guild.
    /// </summary>
    GuildsMembersRead,

    /// <summary>
    /// Allows your app to join users into a group DM.
    /// </summary>
    GdmJoin,

    /// <summary>
    /// For local RPC server access, this allows you to control a user's local Discord client.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    Rpc,

    /// <summary>
    /// For local RPC server access, this allows you to receive notifications pushed to the user.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcNotificationsRead,

    /// <summary>
    /// For local RPC server access, this allows you to read a user's voice settings and listen for voice events.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcVoiceRead,

    /// <summary>
    /// For local RPC server access, this allows you to update a user's voice settings.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcVoiceWrite,

    /// <summary>
    /// For local RPC server access, this allows you to update a user's activity.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcActivitiesWrite,

    /// <summary>
    /// For OAuth2 bots, this puts the bot in the user's selected guild by default.
    /// </summary>
    Bot,

    /// <summary>
    /// This generates a webhook that is returned in the OAuth token response for authorization code grants.
    /// </summary>
    WebhookIncoming,

    /// <summary>
    /// For local RPC server access, this allows you to read messages from all client channels
    /// (otherwise restricted to channels/guilds your application creates).
    /// </summary>
    MessagesRead,

    /// <summary>
    /// Allows your application to upload/update builds for a user's applications.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    ApplicationsBuildsUpload,

    /// <summary>
    /// Allows your application to read build data for a user's applications.
    /// </summary>
    ApplicationsBuildsRead,

    /// <summary>
    /// Allows your application to use application commands in a guild.
    /// </summary>
    ApplicationsCommands,

    /// <summary>
    /// Allows your application to read and update store data (SKUs, store listings, achievements etc.) for a user's applications.
    /// </summary>
    ApplicationsStoreUpdate,

    /// <summary>
    /// Allows your application to read entitlements for a user's applications.
    /// </summary>
    ApplicationsEntitlements,

    /// <summary>
    /// Allows your application to fetch data from a user's "Now Playing/Recently Played" list.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    ActivitiesRead,

    /// <summary>
    /// Allows your application to update a user's activity.
    /// </summary>
    /// <remarks>
    /// Outside of the GameSDK activity manager, this scope requires Discord approval.
    /// </remarks>
    ActivitiesWrite,

    /// <summary>
    /// Allows your application to know a user's friends and implicit relationships.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RelationshipsRead
}
