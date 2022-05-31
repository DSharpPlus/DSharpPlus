using System;
using System.Collections.Generic;
using DSharpPlus.Core.Enums;
using Newtonsoft.Json;

namespace DSharpPlus.Core.RestEntities
{
    public sealed record DiscordApplication
    {
        /// <summary>
        /// The id of the app.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The name of the app.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; init; } = null!;

        /// <summary>
        /// The icon hash of the app.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string? Icon { get; init; }

        /// <summary>
        /// The description of the app.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; init; } = null!;

        /// <summary>
        /// An array of rpc origin urls, if rpc is enabled.
        /// </summary>
        [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<IReadOnlyList<string>> RpcOrigins { get; init; }

        /// <summary>
        /// When false only app owner can join the app's bot to guilds.
        /// </summary>
        [JsonProperty("bot_public", NullValueHandling = NullValueHandling.Ignore)]
        public bool BotPublic { get; init; }

        /// <summary>
        /// When true the app's bot will only join upon completion of the full oauth2 code grant flow.
        /// </summary>
        [JsonProperty("bot_require_code_grant", NullValueHandling = NullValueHandling.Ignore)]
        public bool BotRequireCodeGrant { get; init; }

        /// <summary>
        /// The url of the app's terms of service.
        /// </summary>
        [JsonProperty("terms_of_service_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> TermsOfServiceUrl { get; init; }

        /// <summary>
        /// The url of the app's privacy policy.
        /// </summary>
        [JsonProperty("privacy_policy_url", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> PrivacyPolicyUrl { get; init; }

        /// <summary>
        /// Partial user object containing info on the owner of the application.
        /// </summary>
        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordUser> Owner { get; init; }

        /// <summary>
        /// deprecated: previously if this application was a game sold on Discord, this field would be the summary field for the store page of its primary SKU; now an empty string
        /// </summary>
        [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("deprecated: previously if this application was a game sold on Discord, this field would be the summary field for the store page of its primary SKU; now an empty string")]
        public string Summary { get; set; } = null!;

        /// <summary>
        /// The hex encoded key for verification in interactions and the GameSDK's GetTicket.
        /// </summary>
        [JsonProperty("verify_key", NullValueHandling = NullValueHandling.Ignore)]
        public string VerifyKey { get; init; } = null!;

        /// <summary>
        /// If the application belongs to a team, this will be a list of the members of that team.
        /// </summary>
        [JsonProperty("team", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordTeam? Team { get; init; }

        /// <summary>
        /// If this application is a game sold on Discord, this field will be the guild to which it has been linked.
        /// </summary>
        [JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> GuildId { get; init; }

        /// <summary>
        /// If this application is a game sold on Discord, this field will be the id of the "Game SKU" that is created, if exists.
        /// </summary>
        [JsonProperty("primary_sku_id", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordSnowflake> PrimarySKUId { get; init; }

        /// <summary>
        /// If this application is a game sold on Discord, this field will be the URL slug that links to the store page.
        /// </summary>
        [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> Slug { get; init; }

        /// <summary>
        /// The application's default rich presence invite cover image hash.
        /// </summary>
        [JsonProperty("cover_image", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<string> CoverImage { get; init; }

        /// <summary>
        /// The application's public <see cref="DiscordApplicationFlags">flags</see>.
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public Optional<DiscordApplicationFlags> Flags { get; init; }

        public static implicit operator ulong(DiscordApplication application) => application.Id;
        public static implicit operator DiscordSnowflake(DiscordApplication application) => application.Id;
    }
}
