using DSharpPlus.Core.Attributes;
using DSharpPlus.Core.Enums;
using System.Text.Json.Serialization;

namespace DSharpPlus.Core.Entities
{
    /// <summary>
    /// Implements a <see href="https://discord.com/developers/docs/resources/user#user-object-user-structure">Internal User</see>.
    /// </summary>
    [InternalGatewayPayload("USER_UPDATE")]
    public sealed record InternalUser
    {
        /// <summary>
        /// The user's id, used to identify the user across all of Internal.
        /// </summary>
        [JsonPropertyName("id")]
        public InternalSnowflake Id { get; init; } = null!;

        /// <summary>
        /// The user's username, not unique across the platform.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; init; } = null!;

        /// <summary>
        /// The user's 4-digit discord-tag.
        /// </summary>
        [JsonPropertyName("discriminator")]
        public string Discriminator { get; init; } = null!;

        /// <summary>
        /// The user's avatar hash.
        /// </summary>
        [JsonPropertyName("avatar")]
        public string? Avatar { get; init; }

        /// <summary>
        /// Whether the user belongs to an OAuth2 application.
        /// </summary>
        [JsonPropertyName("bot")]
        public Optional<bool> Bot { get; init; }

        /// <summary>
        /// Whether the user is an Official Internal System user (part of the urgent message system).
        /// </summary>
        [JsonPropertyName("system")]
        public Optional<bool> System { get; init; }

        /// <summary>
        /// Whether the user has two factor enabled on their account.
        /// </summary>
        [JsonPropertyName("mfa_enabled")]
        public Optional<bool> MFAEnabled { get; init; }

        /// <summary>
        /// The user's banner hash.
        /// </summary>
        [JsonPropertyName("banner")]
        public Optional<string?> Banner { get; init; }

        /// <summary>
        /// The user's banner color.
        /// </summary>
        [JsonPropertyName("accent_color")]
        public Optional<int?> AccentColor { get; init; }

        /// <summary>
        /// The user's chosen language option.
        /// </summary>
        [JsonPropertyName("locale")]
        public Optional<string> Locale { get; init; }

        /// <summary>
        /// Whether the email on this account has been verified. Requires the <see cref="InternalApplicationScopes.Email"/> scope.
        /// </summary>
        [JsonPropertyName("verified")]
        public Optional<bool> Verified { get; init; }

        /// <summary>
        /// The user's email. Requires the <see cref="InternalApplicationScopes.Email"/> scope.
        /// </summary>
        [JsonPropertyName("email")]
        public Optional<string?> Email { get; init; }

        /// <summary>
        /// The user flags on a user's account.
        /// </summary>
        [JsonPropertyName("flags")]
        public Optional<InternalUserFlags> Flags { get; init; }

        /// <summary>
        /// The type of Nitro subscription on a user's account.
        /// </summary>
        [JsonPropertyName("premium_type")]
        public Optional<InternalPremiumType> PremiumType { get; init; }

        /// <summary>
        /// The public flags on a user's account.
        /// </summary>
        [JsonPropertyName("public_flags")]
        public Optional<InternalUserFlags> PublicFlags { get; init; }

        /// <summary>
        /// Only set on the <c>MESSAGE_CREATE</c> and <c>MESSAGE_UPDATE</c> gateway payloads.
        /// </summary>
        [JsonPropertyName("member")]
        public Optional<InternalGuildMember> Member { get; init; }

        public static implicit operator ulong(InternalUser user) => user.Id;
        public static implicit operator InternalSnowflake(InternalUser user) => user.Id;
    }
}
