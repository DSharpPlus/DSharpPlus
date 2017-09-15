using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
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
            this._role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._role_ids));
        }

        internal DiscordMember(DiscordUser user)
        {
            this.AvatarHash = user.AvatarHash;
            this.Discord = user.Discord;
            this.Discriminator = user.Discriminator;
            this.Email = user.Email;
            this.Id = user.Id;
            this.IsBot = user.IsBot;
            this.MfaEnabled = user.MfaEnabled;
            this.Username = user.Username;
            this.Verified = user.Verified;
            this._role_ids = new List<ulong>();

            this._role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._role_ids));
        }

        internal DiscordMember(TransportMember mbr)
            : base(mbr.User)
        {
            this.IsDeafened = mbr.IsDeafened;
            this.IsMuted = mbr.IsMuted;
            this.JoinedAt = mbr.JoinedAt;
            this.Nickname = mbr.Nickname;
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
        public string DisplayName => this.Nickname ?? this.Username;

        /// <summary>
        /// List of role ids
        /// </summary>
        [JsonIgnore]
        internal IReadOnlyList<ulong> RoleIds => this._role_ids_lazy.Value;
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal List<ulong> _role_ids;
        [JsonIgnore]
        private Lazy<IReadOnlyList<ulong>> _role_ids_lazy;

        /// <summary>
        /// Gets the list of roles associated with this member.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles => this.RoleIds.Select(xid => this.Guild.Roles.FirstOrDefault(xr => xr.Id == xid));

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
        public DiscordVoiceState VoiceState => this.Discord.Guilds[this._guild_id].VoiceStates.FirstOrDefault(xvs => xvs.UserId == this.Id);
        [JsonIgnore]
        internal ulong _guild_id = 0;

        /// <summary>
        /// Gets the guild of which this member is a part of.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild => this.Discord.Guilds[_guild_id];

        /// <summary>
        /// Gets whether this member is the Guild owner.
        /// </summary>
        [JsonIgnore]
        public bool IsOwner => this.Id == this.Guild.OwnerId;

        /// <summary>
        /// Creates a direct message channel to this member.
        /// </summary>
        /// <returns>Direct message channel to this member.</returns>
        public Task<DiscordDmChannel> CreateDmChannelAsync() => this.Discord.ApiClient.CreateDmAsync(this.Id);

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendMessageAsync(string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            var chn = await this.CreateDmChannelAsync();
            return await chn.SendMessageAsync(content, is_tts, embed);
        }

        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach as a file.</param>
        /// <param name="file_name">Name of the file to attach.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(Stream file_data, string file_name, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            var chn = await this.CreateDmChannelAsync();
            return await chn.SendFileAsync(file_data, file_name, content, is_tts, embed);
        }

#if !NETSTANDARD1_1
        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="file_data">Stream containing the data to attach as a file.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(FileStream file_data, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            var chn = await this.CreateDmChannelAsync();
            return await chn.SendFileAsync(file_data, content, is_tts, embed);
        }

        /// <summary>
        /// Sends a direct message with a file attached to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="file_path">Path to the file to attach to the message.</param>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendFileAsync(string file_path, string content = null, bool is_tts = false, DiscordEmbed embed = null)
        {
            var chn = await this.CreateDmChannelAsync();
            return await chn.SendFileAsync(file_path, content, is_tts, embed);
        }
#endif

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
            var chn = await this.CreateDmChannelAsync();
            return await chn.SendMultipleFilesAsync(files, content, is_tts, embed);
        }

        /// <summary>
        /// Sets this member's voice mute status.
        /// </summary>
        /// <param name="mute">Whether the member is to be muted.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task SetMuteAsync(bool mute, string reason = null) => this.Discord.ApiClient.ModifyGuildMemberAsync(_guild_id, this.Id, null, null, mute, null, null, reason);
        
        /// <summary>
        /// Sets this member's voice deaf status.
        /// </summary>
        /// <param name="deaf">Whether the member is to be deafened.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task SetDeafAsync(bool deaf, string reason = null) => this.Discord.ApiClient.ModifyGuildMemberAsync(_guild_id, this.Id, null, null, null, deaf, null, reason);

        /// <summary>
        /// Modifies this member.
        /// </summary>
        /// <param name="nickname">Nickname to set for this member.</param>
        /// <param name="roles">Roles to set for this member.</param>
        /// <param name="mute">Whether the member is to be muted in voice.</param>
        /// <param name="deaf">Whether the member is to be deafened in voice.</param>
        /// <param name="voice_channel">Voice channel to put the member into.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public async Task ModifyAsync(string nickname = null, IEnumerable<DiscordRole> roles = null, bool? mute = null, bool? deaf = null, DiscordChannel voice_channel = null, string reason = null)
        {
            if (voice_channel != null && voice_channel.Type != ChannelType.Voice)
                throw new ArgumentException("Given channel is not a voice channel.", nameof(voice_channel));

            if (nickname != null && this.Discord.CurrentUser.Id == this.Id)
            {
                await this.Discord.ApiClient.ModifyCurrentMemberNicknameAsync(this.Guild.Id, nickname, reason);
                await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, null, roles != null ? roles.Select(xr => xr.Id) : null, mute, deaf, voice_channel?.Id, reason);
            }
            else
            {
                await this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, nickname, roles != null ? roles.Select(xr => xr.Id) : null, mute, deaf, voice_channel?.Id, reason);
            }
        }

        /// <summary>
        /// Grants a role to the member. 
        /// </summary>
        /// <param name="role">Role to grant.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task GrantRoleAsync(DiscordRole role, string reason = null) =>
            this.Discord.ApiClient.AddGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

        /// <summary>
        /// Revokes a role from a member.
        /// </summary>
        /// <param name="role">Role to revoke.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task RevokeRoleAsync(DiscordRole role, string reason = null) =>
            this.Discord.ApiClient.RemoveGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

        /// <summary>
        /// Sets the member's roles to ones specified.
        /// </summary>
        /// <param name="roles">Roles to set.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task ReplaceRolesAsync(IEnumerable<DiscordRole> roles, string reason = null) =>
            this.Discord.ApiClient.ModifyGuildMemberAsync(this.Guild.Id, this.Id, null, roles.Select(xr => xr.Id), null, null, null, reason);
        
        /// <summary>
        /// Returns a string representation of this member.
        /// </summary>
        /// <returns>String representation of this member.</returns>
        public override string ToString()
        {
            return string.Concat("Member ", this.Id, "; ", this.Username, "#", this.Discriminator, " (", this.DisplayName, ")");
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
        public static bool operator !=(DiscordMember e1, DiscordMember e2) =>
            !(e1 == e2);
    }
}
