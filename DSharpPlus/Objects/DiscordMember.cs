using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Objects.Transport;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a Discord guild member.
    /// </summary>
    public class DiscordMember : DiscordUser
    {
        internal DiscordMember()
        {
            this._role_ids_lazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._role_ids));
        }

        internal DiscordMember(DiscordUser user)
        {
            this.AvatarHash = user.AvatarHash;
            this.Discord = user.Discord;
            this.DiscriminatorInt = user.DiscriminatorInt;
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
        public int Color
        {
            get
            {
                var role = this.Roles.OrderByDescending(xr => xr.Position).FirstOrDefault(xr => xr.Color != 0);
                if (role != null)
                    return role.Color;
                return 0;
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
        public DiscordVoiceState VoiceState => this.Discord.Guilds[this._guild_id].VoiceStates.FirstOrDefault(xvs => xvs.UserId == this.Id);

        /// <summary>
        /// Gets this member's presence.
        /// </summary>
        public new DiscordPresence Presence => this.Guild._presences.FirstOrDefault(xp => xp.User.Id == this.Id) ?? base.Presence;

        /// <summary>
        /// Gets this user's presence.
        /// </summary>
        public DiscordPresence UserPresence => base.Presence;

        internal ulong _guild_id = 0;

        /// <summary>
        /// Gets the guild of which this member is a part of.
        /// </summary>
        public DiscordGuild Guild => this.Discord.Guilds[_guild_id];

        public Task<DiscordDmChannel> CreateDmChannelAsync() => this.Discord._rest_client.InternalCreateDmAsync(this.Id);

        public Task SetMuteAsync(bool mute, string reason = null) => this.Discord._rest_client.InternalModifyGuildMemberAsync(_guild_id, this.Id, null, null, mute, null, null, reason);

        public Task SetDeafAsync(bool deaf, string reason = null) => this.Discord._rest_client.InternalModifyGuildMemberAsync(_guild_id, this.Id, null, null, null, deaf, null, reason);

        public Task ModifyAsync(string nickname = null, IEnumerable<DiscordRole> roles = null, bool? mute = null, bool? deaf = null, DiscordChannel voice_channel = null, string reason = null)
        {
            if (voice_channel != null && voice_channel.Type != ChannelType.Voice)
                throw new ArgumentException("Given channel is not a voice channel.", nameof(voice_channel));

            return this.Discord._rest_client.InternalModifyGuildMemberAsync(this.Guild.Id, this.Id, nickname, roles != null ? roles.Select(xr => xr.Id) : null, mute, deaf, voice_channel?.Id, reason);
        }

        public Task GrantRoleAsync(DiscordRole role, string reason = null) =>
            this.Discord._rest_client.InternalAddGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

        public Task TakeRoleAsync(DiscordRole role, string reason = null) =>
            this.Discord._rest_client.InternalRemoveGuildMemberRoleAsync(this.Guild.Id, this.Id, role.Id, reason);

        public Task ReplaceRolesAsync(IEnumerable<DiscordRole> roles, string reason = null) =>
            this.Discord._rest_client.InternalModifyGuildMemberAsync(this.Guild.Id, this.Id, null, roles.Select(xr => xr.Id), null, null, null, reason);
    }
}
