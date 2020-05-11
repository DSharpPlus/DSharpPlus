using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using DSharpPlus.Net.Abstractions;
using DSharpPlus.Net.Models;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord guild member.
    /// </summary>
    public class DiscordMember : DiscordUser, IEquatable<DiscordMember>
    {
        internal DiscordMember()
        {
            this._role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._role_ids));
        }

        internal DiscordMember(DiscordUser user)
        {
            this.Discord = user.Discord;

            this.Id = user.Id;

            this._role_ids = new List<ulong>();
            this._role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._role_ids));
        }

        internal DiscordMember(TransportMember mbr)
        {
            this.Id = mbr.User.Id;
            this.IsDeafened = mbr.IsDeafened;
            this.IsMuted = mbr.IsMuted;
            this.JoinedAt = mbr.JoinedAt;
            this.Nickname = mbr.Nickname;
            this.PremiumSince = mbr.PremiumSince;

            this._role_ids = mbr.Roles ?? new List<ulong>();
            this._role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._role_ids));
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
        public string DisplayName 
            => this.Nickname ?? this.Username;

        /// <summary>
        /// List of role ids
        /// </summary>
        [JsonIgnore]
        internal IReadOnlyList<ulong> RoleIds 
            => this._role_ids_lazy.Value;

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal List<ulong> _role_ids;
        [JsonIgnore]
        private Lazy<IReadOnlyList<ulong>> _role_ids_lazy;

        /// <summary>
        /// Gets the list of roles associated with this member.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles 
            => this.RoleIds.Select(id => this.Guild.GetRole(id)).Where(x => x != null);

        /// <summary>
        /// Gets the color associated with this user's top color-giving role, otherwise 0 (no color).
        /// </summary>
        [JsonIgnore]
        public DiscordColor Color
        {
            get
            {
                var role = this.Roles.OrderByDescending(xr => xr.Position).FirstOrDefault(xr => xr.Color.Value != 0);
                if (role != null)
                    return role.Color;
                return new DiscordColor();
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
        /// Gets this member's voice state.
        /// </summary>
        [JsonIgnore]
        public DiscordVoiceState VoiceState 
            => this.Discord.Guilds[this._guild_id].VoiceStates.TryGetValue(this.Id, out var voiceState) ? voiceState : null;

        [JsonIgnore]
        internal ulong _guild_id = 0;

        /// <summary>
        /// Gets the guild of which this member is a part of.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild 
            => this.Discord.Guilds[_guild_id];

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

        #region Overridden user properties
        [JsonIgnore]
        internal DiscordUser User 
            => this.Discord.UserCache[this.Id];

        /// <summary>
        /// Gets this member's username.
        /// </summary>
        public override string Username
        {
            get => this.User.Username;
            internal set => this.User.Username = value;
        }

        /// <summary>
        /// Gets the member's 4-digit discriminator.
        /// </summary>
        public override string Discriminator
        {
            get => this.User.Discriminator;
            internal set => this.User.Discriminator = value;
        }

        /// <summary>
        /// Gets the member's avatar hash.
        /// </summary>
        public override string AvatarHash
        {
            get => this.User.AvatarHash;
            internal set => this.User.AvatarHash = value;
        }

        /// <summary>
        /// Gets whether the member is a bot.
        /// </summary>
        public override bool IsBot
        {
            get => this.User.IsBot;
            internal set => this.User.IsBot = value;
        }

        /// <summary>
        /// Gets the member's email address.
        /// <para>This is only present in OAuth.</para>
        /// </summary>
        public override string Email
        {
            get => this.User.Email;
            internal set => this.User.Email = value;
        }

        /// <summary>
        /// Gets whether the member has multi-factor authentication enabled.
        /// </summary>
        public override bool? MfaEnabled
        {
            get => this.User.MfaEnabled;
            internal set => this.User.MfaEnabled = value;
        }

        /// <summary>
        /// Gets whether the member is verified.
        /// <para>This is only present in OAuth.</para>
        /// </summary>
        public override bool? Verified
        {
            get => this.User.Verified;
            internal set => this.User.Verified = value;
        }

        /// <summary>
        /// Gets the member's chosen language
        /// </summary>
        public override string Locale
        {
            get => this.User.Locale;
            internal set => this.User.Locale = value;
        }

        /// <summary>
        /// Gets the user's flags.
        /// </summary>
        public override UserFlags? OAuthFlags 
        { 
            get => this.User.OAuthFlags; 
            internal set => this.User.OAuthFlags = value; 
        }

        /// <summary>
        /// Gets the member's flags for OAuth.
        /// </summary>
        public override UserFlags? Flags 
        { 
            get => this.User.Flags; 
            internal set => this.User.Flags = value; 
        }
        #endregion

        /// <summary>
        /// Creates a direct message channel to this member.
        /// </summary>
        /// <returns>Direct message channel to this member.</returns>
        public Task<DiscordDmChannel> CreateDmChannelAsync() 
            => this.Discord.ApiClient.CreateDmAsync(this.Id);

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendMessageAsync(string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");
            
            var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
            return await chn.SendMessageAsync(content, is_tts, embed).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="fileName">Name of the file to attach.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(string fileName, Stream fileData, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");
            
            var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
            return await chn.SendFileAsync(fileName, fileData, content, is_tts, embed).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="fileData">Stream containing the data to attach as a file.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(FileStream fileData, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");

            var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
            return await chn.SendFileAsync(fileData, content, is_tts, embed).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="filePath">Path to the file to attach to the message.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(string filePath, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");

            var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
            return await chn.SendFileAsync(filePath, content, is_tts, embed).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a direct message with several files attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="files">Dictionary of filename to data stream containing the data to upload as files.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendMultipleFilesAsync(Dictionary<string, Stream> files, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");

            var chn = await this.CreateDmChannelAsync().ConfigureAwait(false);
            return await chn.SendMultipleFilesAsync(files, content, is_tts, embed).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets this member's voice mute status.
        /// </summary>
        /// <param name="mute">Whether the member is to be muted.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task SetMuteAsync(bool mute, string reason = null) 
            => this.Discord.ApiClient.ModifyGuildMemberAsync(_guild_id, this.Id, default, default, mute, default, default, reason);
        
        /// <summary>
        /// Sets this member's voice deaf status.
        /// </summary>
        /// <param name="deaf">Whether the member is to be deafened.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task SetDeafAsync(bool deaf, string reason = null) 
            => this.Discord.ApiClient.ModifyGuildMemberAsync(_guild_id, this.Id, default, default, default, deaf, default, reason);

        /// <summary>
        /// Modifies this member.
        /// </summary>
        /// <param name="action">Action to perform on this member.</param>
        /// <returns></returns>
        public async Task ModifyAsync(Action<MemberEditModel> action)
        {
            var mdl = new MemberEditModel();
            action(mdl);

            if (mdl.VoiceChannel.HasValue && mdl.VoiceChannel.Value != null && mdl.VoiceChannel.Value.Type != ChannelType.Voice)
                throw new ArgumentException("Given channel is not a voice channel.", nameof(mdl.VoiceChannel));

            if (mdl.Nickname.HasValue && this.Discord.CurrentUser.Id == this.Id)
            {
                await this.Discord.ApiClient.ModifyCurrentMemberNicknameAsync(this.Guild.Id, mdl.Nickname.Value,
                    mdl.AuditLogReason).ConfigureAwait(false);

                await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, Optional.FromNoValue<string>(),
                    mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                    mdl.VoiceChannel.IfPresent(e => e?.Id), mdl.AuditLogReason).ConfigureAwait(false);
            }
            else
            {
                await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, mdl.Nickname,
                    mdl.Roles.IfPresent(e => e.Select(xr => xr.Id)), mdl.Muted, mdl.Deafened,
                    mdl.VoiceChannel.IfPresent(e => e?.Id), mdl.AuditLogReason).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Grants a role to the member. 
        /// </summary>
        /// <param name="role">Role to grant.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task GrantRoleAsync(DiscordRole role, string reason = null) 
            => this.Discord.ApiClient.AddGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

        /// <summary>
        /// Revokes a role from a member.
        /// </summary>
        /// <param name="role">Role to revoke.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task RevokeRoleAsync(DiscordRole role, string reason = null) 
            => this.Discord.ApiClient.RemoveGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

        /// <summary>
        /// Sets the member's roles to ones specified.
        /// </summary>
        /// <param name="roles">Roles to set.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task ReplaceRolesAsync(IEnumerable<DiscordRole> roles, string reason = null) 
            => this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, default,
                new Optional<IEnumerable<ulong>>(roles.Select(xr => xr.Id)), default, default, default, reason);

        /// <summary>
        /// Bans a this member from their guild.
        /// </summary>
        /// <param name="delete_message_days">How many days to remove messages from.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task BanAsync(int delete_message_days = 0, string reason = null) 
            => this.Guild.BanMemberAsync(this, delete_message_days, reason);

        public Task UnbanAsync(string reason = null) => this.Guild.UnbanMemberAsync(this, reason);

        /// <summary>
        /// Kicks this member from their guild.
        /// </summary>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <remarks>[alias="KickAsync"]</remarks>
        public Task RemoveAsync(string reason = null)
            => this.Discord.ApiClient.RemoveGuildMemberAsync(this._guild_id, this.Id, reason);

        /// <summary>
        /// Moves this member to the specified voice channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public Task PlaceInAsync(DiscordChannel channel) 
            => channel.PlaceMemberAsync(this);

        /// <summary>
        /// Calculates permissions in a given channel for this member.
        /// </summary>
        /// <param name="channel">Channel to calculate permissions for.</param>
        /// <returns>Calculated permissions for this member in the channel.</returns>
        public Permissions PermissionsIn(DiscordChannel channel) 
            => channel.PermissionsFor(this);

        /// <summary>
        /// Returns a string representation of this member.
        /// </summary>
        /// <returns>String representation of this member.</returns>
        public override string ToString()
        {
            return $"Member {this.Id}; {this.Username}#{this.Discriminator} ({this.DisplayName})";
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMember"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordMember"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordMember);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordMember"/> is equal to another <see cref="DiscordMember"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordMember"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordMember"/> is equal to this <see cref="DiscordMember"/>.</returns>
        public bool Equals(DiscordMember e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id && this._guild_id == e._guild_id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordMember"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordMember"/>.</returns>
        public override int GetHashCode()
        {
            int hash = 13;

            hash = (hash * 7) + this.Id.GetHashCode();
            hash = (hash * 7) + this._guild_id.GetHashCode();

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

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id && e1._guild_id == e2._guild_id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordMember"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First member to compare.</param>
        /// <param name="e2">Second member to compare.</param>
        /// <returns>Whether the two members are not equal.</returns>
        public static bool operator !=(DiscordMember e1, DiscordMember e2) 
            => !(e1 == e2);
    }
}
