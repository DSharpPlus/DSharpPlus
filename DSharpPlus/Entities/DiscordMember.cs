using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using DSharpPlus.Enums;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord guild member.
    /// </summary>
    public class DiscordMember : DiscordUser, IEquatable<DiscordMember>
    {
        internal DiscordMember()
        {
            _roleIdsLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(_roleIds));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        internal DiscordMember(DiscordUser user)
        {
            Discord = user.Discord;

            Id = user.Id;

            _roleIds = new List<ulong>();
            _roleIdsLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(_roleIds));
        }

        internal DiscordMember(TransportMember mbr)
        {
            Id = mbr.User.Id;
            IsDeafened = mbr.IsDeafened;
            IsMuted = mbr.IsMuted;
            JoinedAt = mbr.JoinedAt;
            Nickname = mbr.Nickname;

            _roleIds = mbr.Roles ?? new List<ulong>();
            _roleIdsLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(_roleIds));
        }

        /// <summary>
        /// Gets this member's nickname.
        /// </summary>
        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; internal set; }

        /// <summary>
        /// Gets this member's display name.
        /// </summary>
        [JsonIgnore]
        public string DisplayName => Nickname ?? Username;

        /// <summary>
        /// List of role ids
        /// </summary>
        [JsonIgnore]
        internal IReadOnlyList<ulong> RoleIds => _roleIdsLazy.Value;
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        // ReSharper disable once InconsistentNaming
        internal List<ulong> _roleIds;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<ulong>> _roleIdsLazy;

        /// <summary>
        /// Gets the list of roles associated with this member.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles => RoleIds.Select(xid => Guild.Roles.FirstOrDefault(xr => xr.Id == xid));

        /// <summary>
        /// Gets the color associated with this user's top color-giving role, otherwise 0 (no color).
        /// </summary>
        [JsonIgnore]
        public DiscordColor Color
        {
            get
            {
                var role = Roles.OrderByDescending(xr => xr.Position).FirstOrDefault(xr => xr.Color.Value != 0);
                if (role != null)
                {
                    return role.Color;
                }

                return new DiscordColor();
            }
        }

        /// <summary>
        /// Date the user joined the guild
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset JoinedAt { get; internal set; }

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
        /// Gets this member's voice state.
        /// </summary>
        [JsonIgnore]
        public DiscordVoiceState VoiceState => Discord.Guilds[GuildId].VoiceStates.FirstOrDefault(xvs => xvs.UserId == Id);
        [JsonIgnore]
        internal ulong GuildId = 0;

        /// <summary>
        /// Gets the guild of which this member is a part of.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild => Discord.Guilds[GuildId];

        /// <summary>
        /// Gets whether this member is the Guild owner.
        /// </summary>
        [JsonIgnore]
        public bool IsOwner => Id == Guild.OwnerId;

        #region Overriden user properties
        [JsonIgnore]
        internal DiscordUser User => Discord.UserCache[Id];

        /// <summary>
        /// Gets this member's username.
        /// </summary>
        public override string Username
        {
            get => User.Username;
            internal set => User.Username = value;
        }

        /// <summary>
        /// Gets the user's 4-digit discriminator.
        /// </summary>
        public override string Discriminator
        {
            get => User.Discriminator;
            internal set => User.Username = value;
        }

        /// <summary>
        /// Gets the user's avatar hash.
        /// </summary>
        public override string AvatarHash
        {
            get => User.AvatarHash;
            internal set => User.AvatarHash = value;
        }

        /// <summary>
        /// Gets whether the user is a bot.
        /// </summary>
        public override bool IsBot
        {
            get => User.IsBot;
            internal set => User.IsBot = value;
        }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        public override string Email
        {
            get => User.Email;
            internal set => User.Email = value;
        }

        /// <summary>
        /// Gets whether the user has multi-factor authentication enabled.
        /// </summary>
        public override bool? MfaEnabled
        {
            get => User.MfaEnabled;
            internal set => User.MfaEnabled = value;
        }

        /// <summary>
        /// Gets whether the user is verified.
        /// </summary>
        public override bool? Verified
        {
            get => User.Verified;
            internal set => User.Verified = value;
        }
        #endregion

        /// <summary>
        /// Creates a direct message channel to this member.
        /// </summary>
        /// <returns>Direct message channel to this member.</returns>
        public Task<DiscordDmChannel> CreateDmChannelAsync() => Discord.ApiClient.CreateDmAsync(Id);

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="isTts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendMessageAsync(string content = null, bool isTts = false, DiscordEmbed embed = null)
        {
            var chn = await CreateDmChannelAsync();
            return await chn.SendMessageAsync(content, isTts, embed);
        }

        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="fileName">Name of the file to attach.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="isTts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(Stream fileData, string fileName, string content = null, bool isTts = false, DiscordEmbed embed = null)
        {
            var chn = await CreateDmChannelAsync();
            return await chn.SendFileAsync(fileData, fileName, content, isTts, embed);
        }

#if !NETSTANDARD1_1
        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="isTts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(FileStream fileData, string content = null, bool isTts = false, DiscordEmbed embed = null)
        {
            var chn = await CreateDmChannelAsync();
            return await chn.SendFileAsync(fileData, content, isTts, embed);
        }

        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="filePath">Path to the file to attach to the message.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="isTts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(string filePath, string content = null, bool isTts = false, DiscordEmbed embed = null)
        {
            var chn = await CreateDmChannelAsync();
            return await chn.SendFileAsync(filePath, content, isTts, embed);
        }
#endif

        /// <summary>
        /// Sends a direct message with several files attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="files">Dictionary of filename to data stream containing the data to upload as files.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="isTts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendMultipleFilesAsync(Dictionary<string, Stream> files, string content = null, bool isTts = false, DiscordEmbed embed = null)
        {
            var chn = await CreateDmChannelAsync();
            return await chn.SendMultipleFilesAsync(files, content, isTts, embed);
        }

        /// <summary>
        /// Sets this member's voice mute status.
        /// </summary>
        /// <param name="mute">Whether the member is to be muted.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task SetMuteAsync(bool mute, string reason = null) => Discord.ApiClient.ModifyGuildMemberAsync(GuildId, Id, null, null, mute, null, null, reason);
        
        /// <summary>
        /// Sets this member's voice deaf status.
        /// </summary>
        /// <param name="deaf">Whether the member is to be deafened.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task SetDeafAsync(bool deaf, string reason = null) => Discord.ApiClient.ModifyGuildMemberAsync(GuildId, Id, null, null, null, deaf, null, reason);

        /// <summary>
        /// Modifies this member.
        /// </summary>
        /// <param name="nickname">Nickname to set for this member.</param>
        /// <param name="roles">Roles to set for this member.</param>
        /// <param name="mute">Whether the member is to be muted in voice.</param>
        /// <param name="deaf">Whether the member is to be deafened in voice.</param>
        /// <param name="voiceChannel">Voice channel to put the member into.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public async Task ModifyAsync(string nickname = null, IEnumerable<DiscordRole> roles = null, bool? mute = null, bool? deaf = null, DiscordChannel voiceChannel = null, string reason = null)
        {
            if (voiceChannel != null && voiceChannel.Type != ChannelType.Voice)
            {
                throw new ArgumentException("Given channel is not a voice channel.", nameof(voiceChannel));
            }

            if (nickname != null && Discord.CurrentUser.Id == Id)
            {
                await Discord.ApiClient.ModifyCurrentMemberNicknameAsync(Guild.Id, nickname, reason);
                await Discord.ApiClient.ModifyGuildMemberAsync(Guild.Id, Id, null, roles != null ? roles.Select(xr => xr.Id) : null, mute, deaf, voiceChannel?.Id, reason);
            }
            else
            {
                await Discord.ApiClient.ModifyGuildMemberAsync(Guild.Id, Id, nickname, roles != null ? roles.Select(xr => xr.Id) : null, mute, deaf, voiceChannel?.Id, reason);
            }
        }

        /// <summary>
        /// Grants a role to the member. 
        /// </summary>
        /// <param name="role">Role to grant.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task GrantRoleAsync(DiscordRole role, string reason = null) =>
            Discord.ApiClient.AddGuildMemberRoleAsync(Guild.Id, Id, role.Id, reason);

        /// <summary>
        /// Revokes a role from a member.
        /// </summary>
        /// <param name="role">Role to revoke.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task RevokeRoleAsync(DiscordRole role, string reason = null) =>
            Discord.ApiClient.RemoveGuildMemberRoleAsync(Guild.Id, Id, role.Id, reason);

        /// <summary>
        /// Sets the member's roles to ones specified.
        /// </summary>
        /// <param name="roles">Roles to set.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task ReplaceRolesAsync(IEnumerable<DiscordRole> roles, string reason = null) =>
            Discord.ApiClient.ModifyGuildMemberAsync(Guild.Id, Id, null, roles.Select(xr => xr.Id), null, null, null, reason);

        /// <summary>
        /// Bans a this member from their guild.
        /// </summary>
        /// <param name="deleteMessageDays">How many days to remove messages from.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task BanAsync(int deleteMessageDays = 0, string reason = null) =>
            Guild.BanMemberAsync(this, deleteMessageDays, reason);

        /// <summary>
        /// Kicks this member from their guild.
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task RemoveAsync(string reason = null) =>
            Guild.RemoveMemberAsync(this, reason);

        /// <summary>
        /// Moves this member to the specified voice channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public Task PlaceInAsync(DiscordChannel channel) =>
            channel.PlaceMemberAsync(this);

        /// <summary>
        /// Calculates permissions in a given channel for this member.
        /// </summary>
        /// <param name="channel">Channel to calculate permissions for.</param>
        /// <returns>Calculated permissions for this member in the channel.</returns>
        public Permissions PermissionsIn(DiscordChannel channel) =>
            channel.PermissionsFor(this);

        /// <summary>
        /// Returns a string representation of this member.
        /// </summary>
        /// <returns>String representation of this member.</returns>
        public override string ToString()
        {
            return $"Member {Id}; {Username}#{Discriminator} ({DisplayName})";
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMember"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordMember"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as DiscordMember);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMember"/> is equal to another <see cref="DiscordMember"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordMember"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordMember"/> is equal to this <see cref="DiscordMember"/>.</returns>
        public bool Equals(DiscordMember e)
        {
            if (ReferenceEquals(e, null))
            {
                return false;
            }

            if (ReferenceEquals(this, e))
            {
                return true;
            }

            return Id == e.Id && GuildId == e.GuildId;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordMember"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordMember"/>.</returns>
        public override int GetHashCode()
        {
            var hash = 13;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            hash = hash * 7 + Id.GetHashCode();
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            hash = hash * 7 + GuildId.GetHashCode();

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
            var o1 = e1 as object;
            var o2 = e2 as object;

            if (o1 == null && o2 != null || o1 != null && o2 == null)
            {
                return false;
            }

            if (o1 == null)
            {
                return true;
            }

            return e1.Id == e2.Id && e1.GuildId == e2.GuildId;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordMember"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First member to compare.</param>
        /// <param name="e2">Second member to compare.</param>
        /// <returns>Whether the two members are not equal.</returns>
        public static bool operator !=(DiscordMember e1, DiscordMember e2) =>
            !(e1 == e2);
    }
}
