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
    /// Default scopes and permissions for each supported installation context.
    /// </summary>
    [JsonProperty("integration_types_config")]
    public IReadOnlyDictionary<DiscordApplicationIntegrationType, DiscordApplicationIntegrationTypeConfiguration?> IntegrationTypeConfigurations { get; internal set; }

    private IReadOnlyList<DiscordApplicationAsset>? Assets { get; set; }
    
    /// <summary>
    /// Hex encoded key for verification of http interactions and the GameSDK's GetTicket 
    /// </summary>
    public string VerifyKey { get; internal set; }

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
    /// <param name="permissions">Permissions for your bot. Only required if the <seealso cref="DiscordOAuthScope.Bot"/> scope is passed.</param>
    /// <param name="scopes">OAuth scopes for your application.</param>
    public string GenerateOAuthUri(string? redirectUri = null, DiscordPermissions? permissions = null, params DiscordOAuthScope[] scopes)
    {
        permissions &= PermissionMethods.FULL_PERMS;

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
}
