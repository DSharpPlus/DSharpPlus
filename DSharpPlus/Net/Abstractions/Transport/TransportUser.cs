namespace DSharpPlus.Net.Abstractions;
using DSharpPlus.Entities;

using Newtonsoft.Json;

internal class TransportUser
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
    public DiscordPremiumType? PremiumType { get; internal set; }

    [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
    public string Locale { get; internal set; }

    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUserFlags? OAuthFlags { get; internal set; }

    [JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordUserFlags? Flags { get; internal set; }

    internal TransportUser() { }

    internal TransportUser(TransportUser other)
    {
        Id = other.Id;
        Username = other.Username;
        Discriminator = other.Discriminator;
        GlobalDisplayName = other.GlobalDisplayName;
        AvatarHash = other.AvatarHash;
        BannerHash = other.BannerHash;
        BannerColor = other.BannerColor;
        IsBot = other.IsBot;
        MfaEnabled = other.MfaEnabled;
        Verified = other.Verified;
        Email = other.Email;
        PremiumType = other.PremiumType;
        Locale = other.Locale;
        Flags = other.Flags;
        OAuthFlags = other.OAuthFlags;
    }
}
