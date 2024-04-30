namespace DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Net;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;
using Newtonsoft.Json;

/// <summary>
/// Represents a Discord guild member.
/// </summary>
public class DiscordMember : DiscordUser, IEquatable<DiscordMember>
{
    internal DiscordMember() => _role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(_role_ids));

    internal DiscordMember(DiscordUser user)
    {
        Discord = user.Discord;

        Id = user.Id;

        _role_ids = new List<ulong>();
        _role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(_role_ids));
    }

    internal DiscordMember(TransportMember member)
    {
        Id = member.User.Id;
        IsDeafened = member.IsDeafened;
        IsMuted = member.IsMuted;
        JoinedAt = member.JoinedAt;
        Nickname = member.Nickname;
        PremiumSince = member.PremiumSince;
        IsPending = member.IsPending;
        _avatarHash = member.AvatarHash;
        _role_ids = member.Roles ?? new List<ulong>();
        _role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(_role_ids));
        CommunicationDisabledUntil = member.CommunicationDisabledUntil;
    }

    /// <summary>
    /// Gets the member's avatar for the current guild.
    /// </summary>
    [JsonIgnore]
    public string GuildAvatarHash => _avatarHash;

    /// <summary>
    /// Gets the members avatar url for the current guild.
    /// </summary>
    [JsonIgnore]
    public string GuildAvatarUrl => string.IsNullOrWhiteSpace(GuildAvatarHash) ? null : $"https://cdn.discordapp.com/{Endpoints.GUILDS}/{_guild_id}/{Endpoints.USERS}/{Id}/{Endpoints.AVATARS}/{GuildAvatarHash}.{(GuildAvatarHash.StartsWith("a_") ? "gif" : "png")}?size=1024";

    [JsonIgnore]
    internal string _avatarHash;

    /// <summary>
    /// Gets this member's nickname.
    /// </summary>
    [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
    public string Nickname { get; internal set; }

    /// <summary>
    /// Gets this member's display name.
    /// </summary>
    [JsonIgnore]
    public string DisplayName => Nickname ?? GlobalName ?? Username;

    /// <summary>
    /// How long this member's communication will be suppressed for.
    /// </summary>
    [JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Include)]
    public DateTimeOffset? CommunicationDisabledUntil { get; internal set; }

    /// <summary>
    /// List of role IDs
    /// </summary>
    [JsonIgnore]
    internal IReadOnlyList<ulong> RoleIds => _role_ids_lazy.Value;

    [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
    internal List<ulong> _role_ids;
    [JsonIgnore]
    private readonly Lazy<IReadOnlyList<ulong>> _role_ids_lazy;

    /// <summary>
    /// Gets the list of roles associated with this member.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<DiscordRole> Roles
        => RoleIds.Select(id => Guild.GetRole(id)).Where(x => x != null);

    /// <summary>
    /// Gets the color associated with this user's top color-giving role, otherwise 0 (no color).
    /// </summary>
    [JsonIgnore]
    public DiscordColor Color
    {
        get
        {
            DiscordRole? role = Roles.OrderByDescending(xr => xr.Position).FirstOrDefault(xr => xr.Color.Value != 0);
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
    /// Gets this member's voice state.
    /// </summary>
    [JsonIgnore]
    public DiscordVoiceState VoiceState
        => Discord.Guilds[_guild_id].VoiceStates.TryGetValue(Id, out DiscordVoiceState? voiceState) ? voiceState : null;

    [JsonIgnore]
    internal ulong _guild_id = 0;

    /// <summary>
    /// Gets the guild of which this member is a part of.
    /// </summary>
    [JsonIgnore]
    public DiscordGuild Guild
        => Discord.Guilds[_guild_id];

    /// <summary>
    /// Gets whether this member is the Guild owner.
    /// </summary>
    [JsonIgnore]
    public bool IsOwner
        => Id == Guild.OwnerId;

    /// <summary>
    /// Gets the member's position in the role hierarchy, which is the member's highest role's position. Returns <see cref="int.MaxValue"/> for the guild's owner.
    /// </summary>
    [JsonIgnore]
    public int Hierarchy
        => IsOwner ? int.MaxValue : RoleIds.Count == 0 ? 0 : Roles.Max(x => x.Position);


    /// <summary>
    /// Gets the permissions for the current member.
    /// </summary>
    [JsonIgnore]
    public DiscordPermissions Permissions => GetPermissions();



    #region Overridden user properties
    [JsonIgnore]
    internal DiscordUser User
        => Discord.UserCache[Id];

    /// <summary>
    /// Gets this member's username.
    /// </summary>
    [JsonIgnore]
    public override string Username
    {
        get => User.Username;
        internal set => User.Username = value;
    }

    /// <summary>
    /// Gets the member's 4-digit discriminator.
    /// </summary>
    [JsonIgnore]
    public override string Discriminator
    {
        get => User.Discriminator;
        internal set => User.Discriminator = value;
    }

    /// <summary>
    /// Gets the member's banner hash.
    /// </summary>
    [JsonIgnore]
    public override string BannerHash
    {
        get => User.BannerHash;
        internal set => User.BannerHash = value;
    }

    /// <summary>
    /// The color of this member's banner. Mutually exclusive with <see cref="BannerHash"/>.
    /// </summary>
    [JsonIgnore]
    public override DiscordColor? BannerColor => User.BannerColor;

    /// <summary>
    /// Gets the member's avatar hash.
    /// </summary>
    [JsonIgnore]
    public override string AvatarHash
    {
        get => User.AvatarHash;
        internal set => User.AvatarHash = value;
    }

    /// <summary>
    /// Gets whether the member is a bot.
    /// </summary>
    [JsonIgnore]
    public override bool IsBot
    {
        get => User.IsBot;
        internal set => User.IsBot = value;
    }

    /// <summary>
    /// Gets the member's email address.
    /// <para>This is only present in OAuth.</para>
    /// </summary>
    [JsonIgnore]
    public override string Email
    {
        get => User.Email;
        internal set => User.Email = value;
    }

    /// <summary>
    /// Gets whether the member has multi-factor authentication enabled.
    /// </summary>
    [JsonIgnore]
    public override bool? MfaEnabled
    {
        get => User.MfaEnabled;
        internal set => User.MfaEnabled = value;
    }

    /// <summary>
    /// Gets whether the member is verified.
    /// <para>This is only present in OAuth.</para>
    /// </summary>
    [JsonIgnore]
    public override bool? Verified
    {
        get => User.Verified;
        internal set => User.Verified = value;
    }

    /// <summary>
    /// Gets the member's chosen language
    /// </summary>
    [JsonIgnore]
    public override string Locale
    {
        get => User.Locale;
        internal set => User.Locale = value;
    }

    /// <summary>
    /// Gets the user's flags.
    /// </summary>
    [JsonIgnore]
    public override DiscordUserFlags? OAuthFlags
    {
        get => User.OAuthFlags;
        internal set => User.OAuthFlags = value;
    }

    /// <summary>
    /// Gets the member's flags for OAuth.
    /// </summary>
    [JsonIgnore]
    public override DiscordUserFlags? Flags
    {
        get => User.Flags;
        internal set => User.Flags = value;
    }

    /// <summary>
    /// Gets the member's global display name.
    /// </summary>
    [JsonIgnore]
    public override string? GlobalName
    {
        get => User.GlobalName;
        internal set => User.GlobalName = value;
    }
    #endregion

    /// <summary>
    /// Creates a direct message channel to this member.
    /// </summary>
    /// <returns>Direct message channel to this member.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordDmChannel> CreateDmChannelAsync()
    {
        DiscordDmChannel dm = default;

        if (Discord is DiscordClient dc)
        {
            dm = dc._privateChannels.Values.FirstOrDefault(x => x.Recipients.FirstOrDefault() == this);
        }

        return dm is not null ? dm : await Discord.ApiClient.CreateDmAsync(Id);
    }

    /// <summary>
    /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
    /// </summary>
    /// <param name="content">Content of the message to send.</param>
    /// <returns>The sent message.</returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(string content)
    {
        if (IsBot && Discord.CurrentUser.IsBot)
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
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordEmbed embed)
    {
        if (IsBot && Discord.CurrentUser.IsBot)
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
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(string content, DiscordEmbed embed)
    {
        if (IsBot && Discord.CurrentUser.IsBot)
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
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task<DiscordMessage> SendMessageAsync(DiscordMessageBuilder message)
    {
        if (IsBot && Discord.CurrentUser.IsBot)
        {
            throw new ArgumentException("Bots cannot DM each other.");
        }

        DiscordDmChannel chn = await CreateDmChannelAsync();
        return await chn.SendMessageAsync(message);
    }

    /// <summary>
    /// Times-out a member and restricts their ability to send messages, add reactions, speak in threads, and join voice channels.
    /// </summary>
    /// <param name="until">How long the timeout should last. Set to <see langword="null"/> or a time in the past to remove the timeout.</param>
    /// <param name="reason">Why this member is being restricted.</param>
    public async Task TimeoutAsync(DateTimeOffset? until, string reason = default)
        => await Discord.ApiClient.ModifyGuildMemberAsync(_guild_id, Id, default, default, default, default, default, until, reason);

    /// <summary>
    /// Sets this member's voice mute status.
    /// </summary>
    /// <param name="mute">Whether the member is to be muted.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MuteMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task SetMuteAsync(bool mute, string reason = null)
        => await Discord.ApiClient.ModifyGuildMemberAsync(_guild_id, Id, default, default, mute, default, default, default, reason);

    /// <summary>
    /// Sets this member's voice deaf status.
    /// </summary>
    /// <param name="deaf">Whether the member is to be deafened.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.DeafenMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task SetDeafAsync(bool deaf, string reason = null)
        => await Discord.ApiClient.ModifyGuildMemberAsync(_guild_id, Id, default, default, default, deaf, default, default, reason);

    /// <summary>
    /// Modifies this member.
    /// </summary>
    /// <param name="action">Action to perform on this member.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageNicknames"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task ModifyAsync(Action<MemberEditModel> action)
    {
        MemberEditModel mdl = new MemberEditModel();
        action(mdl);

        if (mdl.VoiceChannel.HasValue && mdl.VoiceChannel.Value != null && mdl.VoiceChannel.Value.Type != DiscordChannelType.Voice && mdl.VoiceChannel.Value.Type != DiscordChannelType.Stage)
        {
            throw new ArgumentException("Given channel is not a voice or stage channel.", nameof(mdl.VoiceChannel));
        }

        if (mdl.Nickname.HasValue && Discord.CurrentUser.Id == Id)
        {
            await Discord.ApiClient.ModifyCurrentMemberAsync(Guild.Id, mdl.Nickname.Value,
                mdl.AuditLogReason);

            await Discord.ApiClient.ModifyGuildMemberAsync(Guild.Id, Id, Optional.FromNoValue<string>(),
                mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                mdl.VoiceChannel.IfPresent(e => e?.Id), default, mdl.AuditLogReason);
        }
        else
        {
            await Discord.ApiClient.ModifyGuildMemberAsync(Guild.Id, Id, mdl.Nickname,
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
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task GrantRoleAsync(DiscordRole role, string reason = null)
        => await Discord.ApiClient.AddGuildMemberRoleAsync(Guild.Id, Id, role.Id, reason);

    /// <summary>
    /// Revokes a role from a member.
    /// </summary>
    /// <param name="role">Role to revoke.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task RevokeRoleAsync(DiscordRole role, string reason = null)
        => await Discord.ApiClient.RemoveGuildMemberRoleAsync(Guild.Id, Id, role.Id, reason);

    /// <summary>
    /// Sets the member's roles to ones specified.
    /// </summary>
    /// <param name="roles">Roles to set.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
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
        IEnumerable<DiscordRole> managedRoles = Roles.Where(x => x.IsManaged);

        IEnumerable<DiscordRole> newRoles = managedRoles.Concat(roles);

        await Discord.ApiClient.ModifyGuildMemberAsync(Guild.Id, Id, default,
            new Optional<IEnumerable<ulong>>(newRoles.Select(xr => xr.Id)), default, default, default, default, reason);
    }


    /// <summary>
    /// Bans a this member from their guild.
    /// </summary>
    /// <param name="delete_message_days">How many days to remove messages from.</param>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task BanAsync(int delete_message_days = 0, string reason = null)
        => Guild.BanMemberAsync(this, delete_message_days, reason);

    /// <exception cref = "Exceptions.UnauthorizedException" > Thrown when the client does not have the<see cref="Permissions.BanMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public Task UnbanAsync(string reason = null) => Guild.UnbanMemberAsync(this, reason);

    /// <summary>
    /// Kicks this member from their guild.
    /// </summary>
    /// <param name="reason">Reason for audit logs.</param>
    /// <returns></returns>
    /// <remarks>[alias="KickAsync"]</remarks>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.KickMembers"/> permission.</exception>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
    /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
    /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
    public async Task RemoveAsync(string reason = null)
        => await Discord.ApiClient.RemoveGuildMemberAsync(_guild_id, Id, reason);

    /// <summary>
    /// Moves this member to the specified voice channel
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MoveMembers"/> permission.</exception>
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

        await Discord.ApiClient.UpdateUserVoiceStateAsync(Guild.Id, Id, channel.Id, suppress);
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
        if (string.IsNullOrWhiteSpace(GuildAvatarHash))
        {
            return GetAvatarUrl(imageFormat, imageSize);
        }

        if (imageFormat == ImageFormat.Unknown)
        {
            throw new ArgumentException("You must specify valid image format.", nameof(imageFormat));
        }

        // Makes sure the image size is in between Discord's allowed range.
        if (imageSize < 16 || imageSize > 4096)
        {
            throw new ArgumentOutOfRangeException("Image Size is not in between 16 and 4096: " + nameof(imageSize));
        }

        // Checks to see if the image size is not a power of two.
        if (!(imageSize is not 0 && (imageSize & (imageSize - 1)) is 0))
        {
            throw new ArgumentOutOfRangeException("Image size is not a power of two: " + nameof(imageSize));
        }

        // Get the string variants of the method parameters to use in the urls.
        string stringImageFormat = imageFormat switch
        {
            ImageFormat.Gif => "gif",
            ImageFormat.Jpeg => "jpg",
            ImageFormat.Png => "png",
            ImageFormat.WebP => "webp",
            ImageFormat.Auto => !string.IsNullOrWhiteSpace(GuildAvatarHash) ? (GuildAvatarHash.StartsWith("a_") ? "gif" : "png") : "png",
            _ => throw new ArgumentOutOfRangeException(nameof(imageFormat)),
        };
        string stringImageSize = imageSize.ToString(CultureInfo.InvariantCulture);

        return $"https://cdn.discordapp.com/{Endpoints.GUILDS}/{_guild_id}/{Endpoints.USERS}/{Id}/{Endpoints.AVATARS}/{GuildAvatarHash}.{stringImageFormat}?size={stringImageSize}";
    }


    /// <summary>
    /// Returns a string representation of this member.
    /// </summary>
    /// <returns>String representation of this member.</returns>
    public override string ToString() => $"Member {Id}; {Username}#{Discriminator} ({DisplayName})";

    /// <summary>
    /// Checks whether this <see cref="DiscordMember"/> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="DiscordMember"/>.</returns>
    public override bool Equals(object obj) => Equals(obj as DiscordMember);

    /// <summary>
    /// Checks whether this <see cref="DiscordMember"/> is equal to another <see cref="DiscordMember"/>.
    /// </summary>
    /// <param name="e"><see cref="DiscordMember"/> to compare to.</param>
    /// <returns>Whether the <see cref="DiscordMember"/> is equal to this <see cref="DiscordMember"/>.</returns>
    public bool Equals(DiscordMember e) => e is not null && (ReferenceEquals(this, e) || (Id == e.Id && _guild_id == e._guild_id));

    /// <summary>
    /// Gets the hash code for this <see cref="DiscordMember"/>.
    /// </summary>
    /// <returns>The hash code for this <see cref="DiscordMember"/>.</returns>
    public override int GetHashCode()
    {
        int hash = 13;

        hash = (hash * 7) + Id.GetHashCode();
        hash = (hash * 7) + _guild_id.GetHashCode();

        return hash;
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordMember"/> objects are equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are equal.</returns>
    public static bool operator ==(DiscordMember e1, DiscordMember e2)
    {
        object? o1 = e1 as object;
        object? o2 = e2 as object;

        return (o1 != null || o2 == null) && (o1 == null || o2 != null)
&& ((o1 == null && o2 == null) || (e1.Id == e2.Id && e1._guild_id == e2._guild_id));
    }

    /// <summary>
    /// Gets whether the two <see cref="DiscordMember"/> objects are not equal.
    /// </summary>
    /// <param name="e1">First member to compare.</param>
    /// <param name="e2">Second member to compare.</param>
    /// <returns>Whether the two members are not equal.</returns>
    public static bool operator !=(DiscordMember e1, DiscordMember e2)
        => !(e1 == e2);

    /// <summary>
    /// Get's the current member's roles based on the sum of the permissions of their given roles.
    /// </summary>
    private DiscordPermissions GetPermissions()
    {
        if (Guild.OwnerId == Id)
        {
            return PermissionMethods.FULL_PERMS;
        }

        DiscordPermissions perms;

        // assign @everyone permissions
        DiscordRole everyoneRole = Guild.EveryoneRole;
        perms = everyoneRole.Permissions;

        // assign permissions from member's roles (in order)
        perms |= Roles.Aggregate(DiscordPermissions.None, (c, role) => c | role.Permissions);

        // Administrator grants all permissions and cannot be overridden
        return (perms & DiscordPermissions.Administrator) == DiscordPermissions.Administrator ? PermissionMethods.FULL_PERMS : perms;
    }
}
