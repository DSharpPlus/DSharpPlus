using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a guild to which the user is invited.
/// </summary>
public class DiscordInviteGuild : SnowflakeObject
{
    /// <summary>
    /// Gets the name of the guild.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the guild icon's hash.
    /// </summary>
    [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
    public string IconHash { get; internal set; }

    /// <summary>
    /// Gets the guild icon's url.
    /// </summary>
    [JsonIgnore]
    public string IconUrl
        => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.jpg" : null;

    /// <summary>
    /// Gets the hash of guild's invite splash.
    /// </summary>
    [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
    internal string SplashHash { get; set; }

    /// <summary>
    /// Gets the URL of guild's invite splash.
    /// </summary>
    [JsonIgnore]
    public string SplashUrl
        => !string.IsNullOrWhiteSpace(this.SplashHash) ? $"https://cdn.discordapp.com/splashes/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.SplashHash}.jpg" : null;

    /// <summary>
    /// Gets the guild's banner hash, when applicable.
    /// </summary>
    [JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
    public string Banner { get; internal set; }

    /// <summary>
    /// Gets the guild's banner in url form.
    /// </summary>
    [JsonIgnore]
    public string BannerUrl
        => !string.IsNullOrWhiteSpace(this.Banner) ? $"https://cdn.discordapp.com/banners/{this.Id}/{this.Banner}" : null;

    /// <summary>
    /// Gets the guild description, when applicable.
    /// </summary>
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; internal set; }

    /// <summary>
    /// Gets a collection of this guild's features.
    /// </summary>
    [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Features { get; internal set; }

    /// <summary>
    /// Gets the guild's verification level.
    /// </summary>
    [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
    public VerificationLevel VerificationLevel { get; internal set; }

    /// <summary>
    /// Gets vanity URL code for this guild, when applicable.
    /// </summary>
    [JsonProperty("vanity_url_code")]
    public string VanityUrlCode { get; internal set; }

    /// <summary>
    /// Gets the guild's welcome screen, when applicable.
    /// </summary>
    [JsonProperty("welcome_screen", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordGuildWelcomeScreen WelcomeScreen { get; internal set; }

    internal DiscordInviteGuild() { }
}
