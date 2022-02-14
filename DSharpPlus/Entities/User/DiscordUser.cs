using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text.Json.Serialization;
using DSharpPlus.Enums;

namespace DSharpPlus.Entities
{
    public sealed class DiscordUser : IEquatable<DiscordUser>
    {
        #region Official Discord Properties
        /// <summary>
        /// The user's id, used to identify the user across all of Discord.
        /// </summary>
        [JsonPropertyName("id")]
        public DiscordSnowflake Id { get; init; }

        /// <summary>
        /// The user's username, not unique across the platform.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; init; }

        /// <summary>
        /// The user's 4-digit discord-tag.
        /// </summary>
        [JsonPropertyName("discriminator")]
        public string Discriminator { get; init; }

        /// <summary>
        /// The user's avatar hash.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string? AvatarHash { get; private set; }

        /// <summary>
        /// Whether the user belongs to an OAuth2 application.
        /// </summary>
        [JsonPropertyName("bot")]
        public bool? IsBot { get; init; }

        /// <summary>
        /// Whether the user is an Official Discord System user (part of the urgent message system).
        /// </summary>
        [JsonPropertyName("system")]
        public bool? IsSystem { get; init; }

        /// <summary>
        /// Whether the user has two factor enabled on their account.
        /// </summary>
        [JsonPropertyName("mfa_enabled")]
        public bool? MFAEnabled { get; private set; }

        /// <summary>
        /// The user's banner hash.
        /// </summary>
        [JsonPropertyName("banner")]
        public string? BannerHash { get; private set; }

        /// <summary>
        /// The user's banner color.
        /// </summary>
        [JsonPropertyName("accent_color")]
        public Color? AccentColor { get; private set; }

        // TODO: Can probably be serialized to a class or dictionary. See https://discord.com/developers/docs/reference#locales
        /// <summary>
        /// The user's chosen language option.
        /// </summary>
        [JsonPropertyName("locale")]
        public string? Locale { get; private set; }

        /// <summary>
        /// Whether the email on this account has been verified. Requires the email oauth2 scope.
        /// </summary>
        [JsonPropertyName("verified")]
        public bool? Verified { get; private set; }

        /// <summary>
        /// The user's email. Requires the email oauth2 scope.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; private set; }

        /// <summary>
        /// The user flags on a user's account.
        /// </summary>
        [JsonPropertyName("flags")]
        public UserFlags? UserFlags { get; private set; }


        /// <summary>
        /// The type of Nitro subscription on a user's account.
        /// </summary>
        [JsonPropertyName("premium_type")]
        public PremiumType? PremiumType { get; private set; }

        /// <summary>
        /// The public flags on a user's account.
        /// </summary>
        [JsonPropertyName("public_flags")]
        public UserFlags? PublicUserFlags { get; private set; }
        #endregion

        #region Generated Properties
        /// <summary>
        /// The <see cref="Discriminator"/> property as an <see cref="int"/>.
        /// </summary>
        [JsonIgnore]
        public int DiscriminatorInt => int.Parse(this.Discriminator, NumberStyles.Integer, CultureInfo.InvariantCulture);

        /// <summary>
        /// The user's banner URL. Null if the user has no banner.
        /// </summary>
        [JsonIgnore]
        public string? BannerUrl => string.IsNullOrEmpty(this.BannerHash) ? null : $"https://cdn.discordapp.com/banners/{this.Id}/{this.BannerHash}.{(this.BannerHash.StartsWith("a") ? "gif" : "png")}?size=4096";

        /// <summary>
        /// The user's avatar URL. If the user doesn't have an avatar set, defaults to the default Discord avatar generated from their discriminator.
        /// </summary>
        [JsonIgnore]
        public string AvatarUrl => !string.IsNullOrWhiteSpace(this.AvatarHash) ? (this.AvatarHash.StartsWith("a_") ? $"https://cdn.discordapp.com/avatars/{this.Id.Value.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHash}.gif?size=4096" : $"https://cdn.discordapp.com/avatars/{this.Id}/{this.AvatarHash}.png?size=4096") : this.DefaultAvatarUrl;

        /// <summary>
        /// The user's default avatar URL, generated from their discriminator.
        /// </summary>
        [JsonIgnore]
        public string DefaultAvatarUrl => $"https://cdn.discordapp.com/embed/avatars/{(this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture)}.png?size=4096";

        /// <summary>
        /// Mention's the user.
        /// </summary>
        [JsonIgnore]
        public string? Mention => $"<@{this.Id}>";

        // TODO: Activate this property when DiscordClient has been implemented.
        //[JsonIgnore]
        //public bool IsCurrentUser => this.Id == Client.CurrentUser.Id;
        #endregion

        #region Constructors
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        // Json conversion will ensure that the properties are valid.
        internal DiscordUser() { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        #endregion

        public override bool Equals(object? obj) => obj is DiscordUser user && this.Id == user.Id;
        public bool Equals(DiscordUser? other) => other is not null && this.Id == other.Id;

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(Username);
            hash.Add(Discriminator);
            hash.Add(AvatarHash);
            hash.Add(IsBot);
            hash.Add(IsSystem);
            hash.Add(MFAEnabled);
            hash.Add(BannerHash);
            hash.Add(AccentColor);
            hash.Add(Locale);
            hash.Add(Verified);
            hash.Add(Email);
            hash.Add(UserFlags);
            hash.Add(PremiumType);
            hash.Add(PublicUserFlags);
            return hash.ToHashCode();
        }
    }
}
