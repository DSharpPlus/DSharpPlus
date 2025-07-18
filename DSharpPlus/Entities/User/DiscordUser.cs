using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord user.
/// </summary>
public class DiscordUser : SnowflakeObject, IEquatable<DiscordUser>
{
    internal DiscordUser() { }
    internal DiscordUser(TransportUser transport)
    {
        this.Id = transport.Id;
        this.Username = transport.Username;
        this.Discriminator = transport.Discriminator;
        this.GlobalName = transport.GlobalDisplayName;
        this.AvatarHash = transport.AvatarHash;
        this.bannerColor = transport.BannerColor;
        this.BannerHash = transport.BannerHash;
        this.IsBot = transport.IsBot;
        this.MfaEnabled = transport.MfaEnabled;
        this.Verified = transport.Verified;
        this.Email = transport.Email;
        this.PremiumType = transport.PremiumType;
        this.Locale = transport.Locale;
        this.Flags = transport.Flags;
        this.OAuthFlags = transport.OAuthFlags;
        this.PrimaryGuild = transport.PrimaryGuild;
    }

    /// <summary>
    /// Gets this user's username.
    /// </summary>
    [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Username { get; internal set; }

    /// <summary>
    /// Gets this user's global display name.
    /// </summary>
    /// <remarks>
    /// A global display name differs from a username in that it acts like a nickname, but is not specific to a single guild.
    /// Nicknames in servers however still take precedence over global names, which take precedence over usernames.
    /// </remarks>
    [JsonProperty("global_name", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string GlobalName { get; internal set; }

    /// <summary>
    /// Gets the user's 4-digit discriminator.
    /// </summary>
    /// <remarks>
    /// As of May 15th, 2023, Discord has begun phasing out discriminators in favor of handles (@username); this property will return "0" for migrated accounts.
    /// </remarks>
    [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Discriminator { get; internal set; }

    [JsonIgnore]
    internal int DiscriminatorInt
        => int.Parse(this.Discriminator, NumberStyles.Integer, CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the user's banner color, if set. Mutually exclusive with <see cref="BannerHash"/>.
    /// </summary>
    public virtual DiscordColor? BannerColor
        => !this.bannerColor.HasValue ? null : new DiscordColor(this.bannerColor.Value);

    [JsonProperty("accent_color")]
    internal int? bannerColor;

    /// <summary>
    /// Gets the user's banner url.
    /// </summary>
    [JsonIgnore]
    public string BannerUrl
        => string.IsNullOrEmpty(this.BannerHash) ? null : $"https://cdn.discordapp.com/banners/{this.Id}/{this.BannerHash}.{(this.BannerHash.StartsWith('a') ? "gif" : "png")}?size=4096";

    /// <summary>
    /// Gets the user's profile banner hash. Mutually exclusive with <see cref="BannerColor"/>.
    /// </summary>
    [JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string BannerHash { get; internal set; }

    /// <summary>
    /// Gets the user's avatar hash.
    /// </summary>
    [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string AvatarHash { get; internal set; }

    /// <summary>
    /// Gets the user's avatar URL.
    /// </summary>
    [JsonIgnore]
    public string AvatarUrl
        => !string.IsNullOrWhiteSpace(this.AvatarHash) ? this.AvatarHash.StartsWith("a_") ? $"https://cdn.discordapp.com/avatars/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.AvatarHash}.gif?size=1024" : $"https://cdn.discordapp.com/avatars/{this.Id}/{this.AvatarHash}.png?size=1024" : this.DefaultAvatarUrl;

    /// <summary>
    /// Gets the URL of default avatar for this user.
    /// </summary>
    [JsonIgnore]
    public string DefaultAvatarUrl
        => $"https://cdn.discordapp.com/embed/avatars/{(this.DiscriminatorInt is 0 ? (this.Id >> 22) % 6 : (ulong)this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture)}.png?size=1024";

    /// <summary>
    /// Gets whether the user is a bot.
    /// </summary>
    [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool IsBot { get; internal set; }

    /// <summary>
    /// Gets whether the user has multi-factor authentication enabled.
    /// </summary>
    [JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? MfaEnabled { get; internal set; }

    /// <summary>
    /// Gets whether the user is an official Discord system user.
    /// </summary>
    [JsonProperty("system", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsSystem { get; internal set; }

    /// <summary>
    /// Gets whether the user is verified.
    /// <para>This is only present in OAuth.</para>
    /// </summary>
    [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? Verified { get; internal set; }

    /// <summary>
    /// Gets the user's email address.
    /// <para>This is only present in OAuth.</para>
    /// </summary>
    [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Email { get; internal set; }

    /// <summary>
    /// Gets the user's premium type.
    /// </summary>
    [JsonProperty("premium_type", NullValueHandling = NullValueHandling.Ignore)]
    public virtual DiscordPremiumType? PremiumType { get; internal set; }

    /// <summary>
    /// Gets the user's chosen language
    /// </summary>
    [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Locale { get; internal set; }

    /// <summary>
    /// Gets the user's flags for OAuth.
    /// </summary>
    [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
    public virtual DiscordUserFlags? OAuthFlags { get; internal set; }

    /// <summary>
    /// Gets the user's flags.
    /// </summary>
    [JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
    public virtual DiscordUserFlags? Flags { get; internal set; }
    
    /// <summary>
    /// The user's primary guild also known as the "guild tag".
    /// </summary>
    [JsonProperty("primary_guild", NullValueHandling = NullValueHandling.Ignore)]
    public virtual DiscordUserPrimaryGuild? PrimaryGuild { get; internal set; }

    /// <summary>
    /// Gets the user's mention string.
    /// </summary>
    [JsonIgnore]
    public string Mention
        => Formatter.Mention(this, this is DiscordMember);

    /// <summary>
    /// Gets whether this user is the Client which created this object.
    /// </summary>
    [JsonIgnore]
    public bool IsCurrent
        => this.Id == this.Discord.CurrentUser.Id;

    /// <summary>
    /// Unbans this user from a guild.
    /// </summary>
    /// <param name="guild">Guild to unban this user from.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task UnbanAsync(DiscordGuild guild, string reason = null)
        => guild.UnbanMemberAsync(this, reason);

    /// <summary>
    /// Gets this user's presence.
    /// </summary>
    [JsonIgnore]
    public DiscordPresence Presence
        => this.Discord is DiscordClient dc ? dc.Presences.TryGetValue(this.Id, out DiscordPresence? presence) ? presence : null : null;

    /// <summary>
    /// Gets the user's avatar URL, in requested format and size.
    /// </summary>
    /// <param name="imageFormat">The image format of the avatar to get.</param>
    /// <param name="imageSize">The maximum size of the avatar. Must be a power of two, minimum 16, maximum 4096.</param>
    /// <returns>The URL of the user's avatar.</returns>
    public string GetAvatarUrl(MediaFormat imageFormat, ushort imageSize = 1024)
    {
        if (imageFormat == MediaFormat.Unknown)
        {
            throw new ArgumentException("You must specify valid image format.", nameof(imageFormat));
        }

        // Makes sure the image size is in between Discord's allowed range.
        if (imageSize is < 16 or > 4096)
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), "Image Size is not in between 16 and 4096: ");
        }

        // Checks to see if the image size is not a power of two.
        if (!(imageSize is not 0 && (imageSize & (imageSize - 1)) is 0))
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), "Image size is not a power of two: ");
        }

        // Get the string variants of the method parameters to use in the urls.
        string stringImageFormat = imageFormat switch
        {
            MediaFormat.Gif => "gif",
            MediaFormat.Jpeg => "jpg",
            MediaFormat.Png => "png",
            MediaFormat.WebP => "webp",
            MediaFormat.Auto => !string.IsNullOrWhiteSpace(this.AvatarHash) ? (this.AvatarHash.StartsWith("a_") ? "gif" : "png") : "png",
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
        };
        string stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);

        // If the avatar hash is set, get their avatar. If it isn't set, grab the default avatar calculated from their discriminator.
        if (!string.IsNullOrWhiteSpace(this.AvatarHash))
        {
            string userId = this.Id.ToString(CultureInfo.InvariantCulture);
            return $"https://cdn.discordapp.com/{Endpoints.AVATARS}/{userId}/{this.AvatarHash}.{stringImageFormat}?size={stringImageSize}";
        }
        else
        {
            // https://discord.com/developers/docs/reference#image-formatting-cdn-endpoints: In the case of the Default User Avatar endpoint, the value for `user_discriminator` in the path should be the user's discriminator `modulo 5—Test#1337` would be `1337 % 5`, which evaluates to 2.
            string defaultAvatarType = (this.DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture);
            return $"https://cdn.discordapp.com/embed/{Endpoints.AVATARS}/{defaultAvatarType}.{stringImageFormat}?size={stringImageSize}";
        }
    }

    /// <summary>
    /// Creates a direct message channel to this member.
    /// </summary>
    /// <returns>Direct message channel to this member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">
    /// Thrown when the member has the bot blocked,
    /// the member does not share a guild with the bot and does not have the user app installed,
    /// or if the member has Allow DM from server members off.
    /// </exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async ValueTask<DiscordDmChannel> CreateDmChannelAsync(bool skipCache = false)
    {
        if (skipCache)
        {
            return await this.Discord.ApiClient.CreateDmAsync(this.Id);
        }

        DiscordDmChannel? dm = default;

        if (this.Discord is DiscordClient dc)
        {
            dm = dc.privateChannels.Values.FirstOrDefault(x => x.Recipients.FirstOrDefault(y => y is not null && y.Id == this.Id) is not null);
        }

        return dm ?? await this.Discord.ApiClient.CreateDmAsync(this.Id);
    }

    /// <summary>
    /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
    /// </summary>
    /// <param name="content">Content of the message to send.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">
    /// Thrown when the member has the bot blocked,
    /// the member does not share a guild with the bot and does not have the user app installed,
    /// or if the member has Allow DM from server members off.
    /// </exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(string content)
    {
        if (this.IsBot && this.Discord.CurrentUser.IsBot)
        {
            throw new ArgumentException("Bots cannot DM each other.");
        }

        DiscordDmChannel chn = await CreateDmChannelAsync();
        return await chn.SendMessageAsync(content);
    }

    /// <summary>
    /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
    /// </summary>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">
    /// Thrown when the member has the bot blocked,
    /// the member does not share a guild with the bot and does not have the user app installed,
    /// or if the member has Allow DM from server members off.
    /// </exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordEmbed embed)
    {
        if (this.IsBot && this.Discord.CurrentUser.IsBot)
        {
            throw new ArgumentException("Bots cannot DM each other.");
        }

        DiscordDmChannel chn = await CreateDmChannelAsync();
        return await chn.SendMessageAsync(embed);
    }

    /// <summary>
    /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
    /// </summary>
    /// <param name="content">Content of the message to send.</param>
    /// <param name="embed">Embed to attach to the message.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">
    /// Thrown when the member has the bot blocked,
    /// the member does not share a guild with the bot and does not have the user app installed,
    /// or if the member has Allow DM from server members off.
    /// </exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed embed)
    {
        if (this.IsBot && this.Discord.CurrentUser.IsBot)
        {
            throw new ArgumentException("Bots cannot DM each other.");
        }

        DiscordDmChannel chn = await CreateDmChannelAsync();
        return await chn.SendMessageAsync(content, embed);
    }

    /// <summary>
    /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
    /// </summary>
    /// <param name="message">Builder to with the message.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">
    /// Thrown when the member has the bot blocked,
    /// the member does not share a guild with the bot and does not have the user app installed,
    /// or if the member has Allow DM from server members off.
    /// </exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordMessageBuilder message)
    {
        if (this.IsBot && this.Discord.CurrentUser.IsBot)
        {
            throw new ArgumentException("Bots cannot DM each other.");
        }

        DiscordDmChannel chn = await CreateDmChannelAsync();
        return await chn.SendMessageAsync(message);
    }

    /// <summary>
    /// Returns a string representation of this user.
    /// </summary>
    /// <returns>String representation of this user.</returns>
    public override string ToString() => $"User {this.Id}; {this.Username}#{this.Discriminator}";

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordUser"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordUser"/>.</returns>
    public override int GetHashCode() => this.Id.GetHashCode();

    /// <summary>
    /// Checks whether this <see cref="DiscordUser"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordUser"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as DiscordUser);

    /// <summary>
    /// Checks whether this <see cref="DiscordUser"/> is equal to another <see cref="DiscordUser"/>.
    /// </summary>
    /// <param name="other"><see cref="DiscordUser"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordUser"/> is equal to this <see cref="DiscordUser"/>.</returns>
    public bool Equals(DiscordUser? other) => this.Id == other?.Id;

    /// <summary>
    /// Gets whether the two <see cref="DiscordUser"/> objects are equal.
    /// </summary>
    /// <param name="obj">First user to compare.</param>
    /// <param name="other">Second user to compare.</param>
    /// <returns>Whether the two users are equal.</returns>
    public static bool operator ==(DiscordUser? obj, DiscordUser? other) => obj?.Equals(other) ?? other is null;

    /// <summary>
    /// Gets whether the two <see cref="DiscordUser"/> objects are not equal.
    /// </summary>
    /// <param name="obj">First user to compare.</param>
    /// <param name="other">Second user to compare.</param>
    /// <returns>Whether the two users are not equal.</returns>
    public static bool operator !=(DiscordUser? obj, DiscordUser? other) => !(obj == other);
}

internal class DiscordUserComparer : IEqualityComparer<DiscordUser>
{
    public bool Equals(DiscordUser x, DiscordUser y) => x.Equals(y);

    public int GetHashCode(DiscordUser obj) => obj.Id.GetHashCode();
}
