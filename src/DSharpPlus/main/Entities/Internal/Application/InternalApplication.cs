using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DSharpPlus.Entities.Internal;

public sealed record InternalApplication
{
    /// <summary>
    /// The id of the app.
    /// </summary>
    [JsonPropertyName("id")]
    public Snowflake Id { get; init; } = null!;

    /// <summary>
    /// The name of the app.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    /// <summary>
    /// The icon hash of the app.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    /// <summary>
    /// The description of the app.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = null!;

    /// <summary>
    /// An array of rpc origin urls, if rpc is enabled.
    /// </summary>
    [JsonPropertyName("rpc_origins")]
    public Optional<IReadOnlyList<string>> RpcOrigins { get; init; }

    /// <summary>
    /// When false only app owner can join the app's bot to guilds.
    /// </summary>
    [JsonPropertyName("bot_public")]
    public bool BotPublic { get; init; }

    /// <summary>
    /// When true the app's bot will only join upon completion of the full oauth2 code grant flow.
    /// </summary>
    [JsonPropertyName("bot_require_code_grant")]
    public bool BotRequireCodeGrant { get; init; }

    /// <summary>
    /// The url of the app's terms of service.
    /// </summary>
    [JsonPropertyName("terms_of_service_url")]
    public Optional<string> TermsOfServiceUrl { get; init; }

    /// <summary>
    /// The url of the app's privacy policy.
    /// </summary>
    [JsonPropertyName("privacy_policy_url")]
    public Optional<string> PrivacyPolicyUrl { get; init; }

    /// <summary>
    /// Partial user object containing info on the owner of the application.
    /// </summary>
    [JsonPropertyName("owner")]
    public Optional<InternalUser> Owner { get; init; }

    /// <summary>
    /// deprecated: previously if this application was a game sold on Internal, this field would be the summary field for the store page of its primary SKU; now an empty string
    /// </summary>
    [JsonPropertyName("summary")]
    [Obsolete("deprecated: previously if this application was a game sold on Internal, this field would be the summary field for the store page of its primary SKU; now an empty string")]
    public string Summary { get; set; } = null!;

    /// <summary>
    /// The hex encoded key for verification in interactions and the GameSDK's GetTicket.
    /// </summary>
    [JsonPropertyName("verify_key")]
    public string VerifyKey { get; init; } = null!;

    /// <summary>
    /// If the application belongs to a team, this will be a list of the members of that team.
    /// </summary>
    [JsonPropertyName("team")]
    public InternalTeam? Team { get; init; }

    /// <summary>
    /// If this application is a game sold on Internal, this field will be the guild to which it has been linked.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public Optional<Snowflake> GuildId { get; init; }

    /// <summary>
    /// If this application is a game sold on Internal, this field will be the id of the "Game SKU" that is created, if exists.
    /// </summary>
    [JsonPropertyName("primary_sku_id")]
    public Optional<Snowflake> PrimarySKUId { get; init; }

    /// <summary>
    /// If this application is a game sold on Internal, this field will be the URL slug that links to the store page.
    /// </summary>
    [JsonPropertyName("slug")]
    public Optional<string> Slug { get; init; }

    /// <summary>
    /// The application's default rich presence invite cover image hash.
    /// </summary>
    [JsonPropertyName("cover_image")]
    public Optional<string> CoverImage { get; init; }

    /// <summary>
    /// The application's public <see cref="DiscordApplicationFlags">flags</see>.
    /// </summary>
    [JsonPropertyName("flags")]
    public Optional<DiscordApplicationFlags> Flags { get; init; }

    /// <summary>
    /// Up to 5 tags describing the content and functionality of the application.
    /// </summary>
    [JsonPropertyName("tags")]
    public Optional<IReadOnlyList<string>> Tags { get; init; }

    /// <summary>
    /// The application's default custom authorization link, if enabled.
    /// </summary>
    [JsonPropertyName("install_params")]
    public Optional<InternalApplicationInstallParameters> InstallParams { get; init; }

    [JsonPropertyName("custom_install_url")]
    public Optional<string> CustomInstallUrl { get; init; }

    public static implicit operator ulong(InternalApplication application) => application.Id;
    public static implicit operator Snowflake(InternalApplication application) => application.Id;
}
