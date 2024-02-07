using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions;

internal sealed record TransportUser
{
    [JsonProperty("id")]
    public ulong Id { get; internal set; }

    [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
    public string Username { get; internal set; }

    [JsonProperty("global_name", NullValueHandling = NullValueHandling.Ignore)]
    public string GlobalDisplayName { get; internal set; }

    [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
    public string Discriminator { get; set; }

    [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
    public string AvatarHash { get; internal set; }

    [JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
    public string BannerHash { get; internal set; }

    [JsonProperty("accent_color")]
    public int? BannerColor { get; internal set; }

    [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsBot { get; internal set; }

    [JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public bool? MfaEnabled { get; internal set; }

    [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Verified { get; internal set; }

    [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
    public string Email { get; internal set; }

    [JsonProperty("premium_type", NullValueHandling = NullValueHandling.Ignore)]
    public PremiumType? PremiumType { get; internal set; }

    [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
    public string Locale { get; internal set; }

    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public UserFlags? OAuthFlags { get; internal set; }

    [JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
    public UserFlags? Flags { get; internal set; }

    internal TransportUser() { }

    internal TransportUser(TransportUser other)
    {
        this.Id = other.Id;
        this.Username = other.Username;
        this.Discriminator = other.Discriminator;
        this.GlobalDisplayName = other.GlobalDisplayName;
        this.AvatarHash = other.AvatarHash;
        this.BannerHash = other.BannerHash;
        this.BannerColor = other.BannerColor;
        this.IsBot = other.IsBot;
        this.MfaEnabled = other.MfaEnabled;
        this.Verified = other.Verified;
        this.Email = other.Email;
        this.PremiumType = other.PremiumType;
        this.Locale = other.Locale;
        this.Flags = other.Flags;
        this.OAuthFlags = other.OAuthFlags;
    }
}
