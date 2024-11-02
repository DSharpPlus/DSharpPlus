using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

namespace DSharpPlus.Entities;

/// <summary>
/// Represents a Discord guild member.
/// </summary>
public class DiscordMember : DiscordUser, IEquatable<DiscordMember>
{
    internal DiscordMember() { }

    internal DiscordMember(DiscordUser user)
    {
        this.Discord = user.Discord;

        this.Id = user.Id;

        this.role_ids = [];
    }

    internal DiscordMember(TransportMember member)
    {
        this.Id = member.User.Id;
        this.IsDeafened = member.IsDeafened;
        this.IsMuted = member.IsMuted;
        this.JoinedAt = member.JoinedAt;
        this.Nickname = member.Nickname;
        this.PremiumSince = member.PremiumSince;
        this.IsPending = member.IsPending;
        this.avatarHash = member.AvatarHash;
        this.role_ids = member.Roles ?? [];
        this.CommunicationDisabledUntil = member.CommunicationDisabledUntil;
    }

    /// <summary>
    /// Gets the member's avatar for the current guild.
    /// </summary>
    [JsonIgnore]
    public string? GuildAvatarHash => this.avatarHash;

    /// <summary>
    /// Gets the members avatar url for the current guild.
    /// </summary>
    [JsonIgnore]
    public string? GuildAvatarUrl => string.IsNullOrWhiteSpace(this.GuildAvatarHash) ? null : $"https://cdn.discordapp.com/{Endpoints.GUILDS}/{this.guild_id}/{Endpoints.USERS}/{this.Id}/{Endpoints.AVATARS}/{this.GuildAvatarHash}.{(this.GuildAvatarHash.StartsWith("a_") ? "gif" : "png")}?size=1024";

    [JsonIgnore]
    internal string? avatarHash;

    /// <summary>
    /// Gets the member's avatar hash as displayed in the current guild.
    /// </summary>
    [JsonIgnore]
    public string DisplayAvatarHash => this.GuildAvatarHash ?? this.User.AvatarHash;

    /// <summary>
    /// Gets the member's avatar url as displayed in the current guild.
    /// </summary>
    [JsonIgnore]
    public string DisplayAvatarUrl => this.GuildAvatarUrl ?? this.User.AvatarUrl;

    /// <summary>
    /// Gets this member's nickname.
    /// </summary>
    [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
    public string Nickname { get; internal set; }

    /// <summary>
    /// Gets this member's display name.
    /// </summary>
    [JsonIgnore]
    public string DisplayName => this.Nickname ?? this.GlobalName ?? this.Username;

    /// <summary>
    /// How long this member's communication will be suppressed for.
    /// </summary>
    [JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Include)]
    public DateTimeOffset? CommunicationDisabledUntil { get; internal set; }

    /// <summary>
    /// List of role IDs
    /// </summary>
    [JsonIgnore]
    internal IReadOnlyList<ulong> RoleIds => this.role_ids;

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    internal List<ulong> role_ids;

    /// <summary>
    /// Gets the list of roles associated with this member.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<DiscordRole> Roles
        => this.RoleIds.Select(id => this.Guild.Roles.GetValueOrDefault(id)).Where(x => x != null);

    /// <summary>
    /// Gets the color associated with this user's top color-giving role, otherwise 0 (no color).
    /// </summary>
    [JsonIgnore]
    public DiscordColor Color
    {
        get
        {
            DiscordRole? role = this.Roles.OrderByDescending(xr => xr.Position).FirstOrDefault(xr => xr.Color.Value != 0);
            return role != null ? role.Color : new DiscordColor();
        }
    }

    /// <summary>
    /// Date the user joined the guild
    /// </summary>
    [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset JoinedAt { get; internal set; }

    /// <summary>
    /// Date the user started boosting this server
    /// </summary>
    [JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? PremiumSince { get; internal set; }

    /// <summary>
    /// If the user is deafened
    /// </summary>
    [JsonProperty("is_deafened", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsDeafened { get; internal set; }

    /// <summary>
    /// If the user is muted
    /// </summary>
    [JsonProperty("is_muted", NullValueHandling = NullValueHandling.Ignore)]
    public bool IsMuted { get; internal set; }

    /// <summary>
    /// If the user has passed the guild's Membership Screening requirements
    /// </summary>
    [JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsPending { get; internal set; }

    /// <summary>
    /// Gets whether or not the member is timed out.
    /// </summary>
    [JsonIgnore]
    public bool IsTimedOut => this.CommunicationDisabledUntil.HasValue && this.CommunicationDisabledUntil.Value > DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets this member's voice state.
    /// </summary>
    [JsonIgnore]
    public DiscordVoiceState VoiceState
        => this.Discord.Guilds[this.guild_id].VoiceStates.TryGetValue(this.Id, out DiscordVoiceState? voiceState) ? voiceState : null;

    [JsonIgnore]
    internal ulong guild_id = 0;

    /// <summary>
    /// Gets the guild of which this member is a part of.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => this.Discord.Guilds[this.guild_id];

    /// <summary>
    /// Gets whether this member is the Guild owner.
    /// </summary>
    [JsonIgnore]
    public bool IsOwner
        => this.Id == this.Guild.OwnerId;

    /// <summary>
    /// Gets the member's position in the role hierarchy, which is the member's highest role's position. Returns <see cref="int.MaxValue"/> for the guild's owner.
    /// </summary>
    [JsonIgnore]
    public int Hierarchy
        => this.IsOwner ? int.MaxValue : this.RoleIds.Count == 0 ? 0 : this.Roles.Max(x => x.Position);

    /// <summary>
    /// Gets the permissions for the current member.
    /// </summary>
    [JsonIgnore]
    public DiscordPermissions Permissions => GetPermissions();


    #region Overridden user properties
    [JsonIgnore]
    internal DiscordUser User
        => this.Discord.UserCache[this.Id];

    /// <summary>
    /// Gets this member's username.
    /// </summary>
    [JsonIgnore]
    public override string Username
    {
        get => this.User.Username;
        internal set => this.User.Username = value;
    }

    /// <summary>
    /// Gets the member's 4-digit discriminator.
    /// </summary>
    [JsonIgnore]
    public override string Discriminator
    {
        get => this.User.Discriminator;
        internal set => this.User.Discriminator = value;
    }

    /// <summary>
    /// Gets the member's banner hash.
    /// </summary>
    [JsonIgnore]
    public override string BannerHash
    {
        get => this.User.BannerHash;
        internal set => this.User.BannerHash = value;
    }

    /// <summary>
    /// The color of this member's banner. Mutually exclusive with <see cref="BannerHash"/>.
    /// </summary>
    [JsonIgnore]
    public override DiscordColor? BannerColor => this.User.BannerColor;

    /// <summary>
    /// Gets the member's avatar hash.
    /// </summary>
    [JsonIgnore]
    public override string AvatarHash
    {
        get => this.User.AvatarHash;
        internal set => this.User.AvatarHash = value;
    }

    /// <summary>
    /// Gets whether the member is a bot.
    /// </summary>
    [JsonIgnore]
    public override bool IsBot
    {
        get => this.User.IsBot;
        internal set => this.User.IsBot = value;
    }

    /// <summary>
    /// Gets the member's email address.
    /// <para>This is only present in OAuth.</para>
    /// </summary>
    [JsonIgnore]
    public override string Email
    {
        get => this.User.Email;
        internal set => this.User.Email = value;
    }

    /// <summary>
    /// Gets whether the member has multi-factor authentication enabled.
    /// </summary>
    [JsonIgnore]
    public override bool? MfaEnabled
    {
        get => this.User.MfaEnabled;
        internal set => this.User.MfaEnabled = value;
    }

    /// <summary>
    /// Gets whether the member is verified.
    /// <para>This is only present in OAuth.</para>
    /// </summary>
    [JsonIgnore]
    public override bool? Verified
    {
        get => this.User.Verified;
        internal set => this.User.Verified = value;
    }

    /// <summary>
    /// Gets the member's chosen language
    /// </summary>
    [JsonIgnore]
    public override string Locale
    {
        get => this.User.Locale;
        internal set => this.User.Locale = value;
    }

    /// <summary>
    /// Gets the user's flags.
    /// </summary>
    [JsonIgnore]
    public override DiscordUserFlags? OAuthFlags
    {
        get => this.User.OAuthFlags;
        internal set => this.User.OAuthFlags = value;
    }

    /// <summary>
    /// Gets the member's flags for OAuth.
    /// </summary>
    [JsonIgnore]
    public override DiscordUserFlags? Flags
    {
        get => this.User.Flags;
        internal set => this.User.Flags = value;
    }

    /// <summary>
    /// Gets the member's global display name.
    /// </summary>
    [JsonIgnore]
    public override string? GlobalName
    {
        get => this.User.GlobalName;
        internal set => this.User.GlobalName = value;
    }
    #endregion

    /// <summary>
    /// Times-out a member and restricts their ability to send messages, add reactions, speak in threads, and join voice channels.
    /// </summary>
    /// <param name="until">How long the timeout should last. Set to <see langword="null"/> or a time in the past to remove the timeout.</param>
    /// <param name="reason">Why this member is being restricted.</param>
    public async Task TimeoutAsync(DateTimeOffset? until, string reason = default)
        => await this.Discord.ApiClient.ModifyGuildMemberAsync(this.guild_id, this.Id, default, default, default, default, default, until, reason);

    /// <summary>
    /// Sets this member's voice mute status.
    /// </summary>
    /// <param name="mute">Whether the member is to be muted.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.MuteMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task SetMuteAsync(bool mute, string reason = null)
        => await this.Discord.ApiClient.ModifyGuildMemberAsync(this.guild_id, this.Id, default, default, mute, default, default, default, reason);

    /// <summary>
    /// Sets this member's voice deaf status.
    /// </summary>
    /// <param name="deaf">Whether the member is to be deafened.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.DeafenMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task SetDeafAsync(bool deaf, string reason = null)
        => await this.Discord.ApiClient.ModifyGuildMemberAsync(this.guild_id, this.Id, default, default, default, deaf, default, default, reason);

    /// <summary>
    /// Modifies this member.
    /// </summary>
    /// <param name="action">Action to perform on this member.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageNicknames"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ModifyAsync(Action<MemberEditModel> action)
    {
        MemberEditModel mdl = new();
        action(mdl);

        if (mdl.VoiceChannel.HasValue && mdl.VoiceChannel.Value != null && mdl.VoiceChannel.Value.Type != DiscordChannelType.Voice && mdl.VoiceChannel.Value.Type != DiscordChannelType.Stage)
        {
            throw new ArgumentException($"{nameof(MemberEditModel)}.{nameof(mdl.VoiceChannel)} must be a voice or stage channel.", nameof(action));
        }

        if (mdl.Nickname.HasValue && this.Discord.CurrentUser.Id == this.Id)
        {
            await this.Discord.ApiClient.ModifyCurrentMemberAsync(this.Guild.Id, mdl.Nickname.Value,
                mdl.AuditLogReason);

            await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, Optional.FromNoValue<string>(),
                mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                mdl.VoiceChannel.IfPresent(e => e?.Id), default, mdl.AuditLogReason);
        }
        else
        {
            await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, mdl.Nickname,
                mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                mdl.VoiceChannel.IfPresent(e => e?.Id), mdl.CommunicationDisabledUntil, mdl.AuditLogReason);
        }
    }

    /// <summary>
    /// Grants a role to the member.
    /// </summary>
    /// <param name="role">Role to grant.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task GrantRoleAsync(DiscordRole role, string reason = null)
        => await this.Discord.ApiClient.AddGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

    /// <summary>
    /// Revokes a role from a member.
    /// </summary>
    /// <param name="role">Role to revoke.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task RevokeRoleAsync(DiscordRole role, string reason = null)
        => await this.Discord.ApiClient.RemoveGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

    /// <summary>
    /// Sets the member's roles to ones specified.
    /// </summary>
    /// <param name="roles">Roles to set.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to add a managed role.</exception>
    public async Task ReplaceRolesAsync(IEnumerable<DiscordRole> roles, string reason = null)
    {
        if (roles.Where(x => x.IsManaged).Any())
        {
            throw new InvalidOperationException("Cannot assign managed roles.");
        }
        IEnumerable<DiscordRole> managedRoles = this.Roles.Where(x => x.IsManaged);

        IEnumerable<DiscordRole> newRoles = managedRoles.Concat(roles);

        await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, default,
            new Optional<IEnumerable<ulong>>(newRoles.Select(xr => xr.Id)), default, default, default, default, reason);
    }

    /// <summary>
    /// Bans a this member from their guild.
    /// </summary>
    /// <param name="deleteMessageDuration">The duration in which discord should delete messages from the banned user.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task BanAsync(TimeSpan deleteMessageDuration = default, string reason = null)
        => this.Guild.BanMemberAsync(this, deleteMessageDuration, reason);

    /// <exception cref = "Exceptions.UnauthorizedException" > Thrown when the client does not have the<see cref="DiscordPermission.BanMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task UnbanAsync(string reason = null) => this.Guild.UnbanMemberAsync(this, reason);

    /// <summary>
    /// Kicks this member from their guild.
    /// </summary>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <remarks>[alias="KickAsync"]</remarks>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.KickMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task RemoveAsync(string reason = null)
        => await this.Discord.ApiClient.RemoveGuildMemberAsync(this.guild_id, this.Id, reason);

    /// <summary>
    /// Moves this member to the specified voice channel
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="DiscordPermission.MoveMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task PlaceInAsync(DiscordChannel channel)
        => channel.PlaceMemberAsync(this);

    /// <summary>
    /// Updates the member's suppress state in a stage channel.
    /// </summary>
    /// <param name="channel">The channel the member is currently in.</param>
    /// <param name="suppress">Toggles the member's suppress state.</param>
    /// <exception cref="ArgumentException">Thrown when the channel in not a voice channel.</exception>
    public async Task UpdateVoiceStateAsync(DiscordChannel channel, bool? suppress)
    {
        if (channel.Type != DiscordChannelType.Stage)
        {
            throw new ArgumentException("Voice state can only be updated in a stage channel.");
        }

        await this.Discord.ApiClient.UpdateUserVoiceStateAsync(this.Guild.Id, this.Id, channel.Id, suppress);
    }

    /// <summary>
    /// Calculates permissions in a given channel for this member.
    /// </summary>
    /// <param name="channel">Channel to calculate permissions for.</param>
    /// <returns>Calculated permissions for this member in the channel.</returns>
    public DiscordPermissions PermissionsIn(DiscordChannel channel)
        => channel.PermissionsFor(this);

    /// <summary>
    /// Constructs the url for a guild member's avatar, defaulting to the user's avatar if none is set.
    /// </summary>
    /// <param name="imageFormat">The image format of the avatar to get.</param>
    /// <param name="imageSize">The maximum size of the avatar. Must be a power of two, minimum 16, maximum 4096.</param>
    /// <returns>The URL of the user's avatar.</returns>
    public string GetGuildAvatarUrl(ImageFormat imageFormat, ushort imageSize = 1024)
    {
        // Run this if statement before any others to prevent running the if statements twice.
        if (string.IsNullOrWhiteSpace(this.GuildAvatarHash))
        {
            return GetAvatarUrl(imageFormat, imageSize);
        }

        if (imageFormat == ImageFormat.Unknown)
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
            ImageFormat.Gif => "gif",
            ImageFormat.Jpeg => "jpg",
            ImageFormat.Png => "png",
            ImageFormat.WebP => "webp",
            ImageFormat.Auto => !string.IsNullOrWhiteSpace(this.GuildAvatarHash) ? (this.GuildAvatarHash.StartsWith("a_") ? "gif" : "png") : "png",
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
        };
        string stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);

        return $"https://cdn.discordapp.com/{Endpoints.GUILDS}/{this.guild_id}/{Endpoints.USERS}/{this.Id}/{Endpoints.AVATARS}/{this.GuildAvatarHash}.{stringImageFormat}?size={stringImageSize}";
    }

    /// <summary>
    /// Returns a string representation of this member.
    /// </summary>
    /// <returns>String representation of this member.</returns>
    public override string ToString() => $"Member {this.Id}; {this.Username}#{this.Discriminator} ({this.DisplayName})";

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordMember"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordMember"/>.</returns>
    public override int GetHashCode()
    {
        int hash = 13;

        hash = (hash * 7) + this.Id.GetHashCode();
        hash = (hash * 7) + this.guild_id.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Checks whether this <see cref="DiscordMember"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordMember"/>.</returns>
    public override bool Equals(object? obj) => Equals(obj as DiscordMember);

    /// <summary>
    /// Checks whether this <see cref="DiscordMember"/> is equal to another <see cref="DiscordMember"/>.
    /// </summary>
    /// <param name="other"><see cref="DiscordMember"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordMember"/> is equal to this <see cref="DiscordMember"/>.</returns>
    public bool Equals(DiscordMember? other) => base.Equals(other) && this.guild_id == other?.guild_id;

    /// <summary>
    /// Gets whether the two <see cref="DiscordMember"/> objects are equal.
    /// </summary>
    /// <param name="obj">First member to compare.</param>
    /// <param name="other">Second member to compare.</param>
    /// <returns>Whether the two members are equal.</returns>
    public static bool operator ==(DiscordMember obj, DiscordMember other) => obj?.Equals(other) ?? other is null;

    /// <summary>
    /// Gets whether the two <see cref="DiscordMember"/> objects are not equal.
    /// </summary>
    /// <param name="obj">First member to compare.</param>
    /// <param name="other">Second member to compare.</param>
    /// <returns>Whether the two members are not equal.</returns>
    public static bool operator !=(DiscordMember obj, DiscordMember other) => !(obj == other);

    /// <summary>
    /// Get's the current member's roles based on the sum of the permissions of their given roles.
    /// </summary>
    private DiscordPermissions GetPermissions()
    {
        if (this.Guild.OwnerId == this.Id)
        {
            return DiscordPermissions.All;
        }

        DiscordPermissions perms;

        // assign @everyone permissions
        DiscordRole everyoneRole = this.Guild.EveryoneRole;
        perms = everyoneRole.Permissions;

        // assign permissions from member's roles (in order)
        perms |= this.Roles.Aggregate(DiscordPermissions.None, (c, role) => c | role.Permissions);

        // Administrator grants all permissions and cannot be overridden
        return perms.HasPermission(DiscordPermission.Administrator) ? DiscordPermissions.All : perms;
    }
}
