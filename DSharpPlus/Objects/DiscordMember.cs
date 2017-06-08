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
    /// 
    /// </summary>
    public class DiscordMember : DiscordUser
    {
        internal DiscordMember() { }
        internal DiscordMember(DiscordUser user)
        {
            this.AvatarHash = user.AvatarHash;
            this.Discord = user.Discord;
            this.DiscriminatorInt = user.DiscriminatorInt;
            this.Email = user.Email;
            this.Id = user.Id;
            this.IsBot = user.IsBot;
            this.MFAEnabled = user.MFAEnabled;
            this.Username = user.Username;
            this.Verified = user.Verified;
        }
        internal DiscordMember(TransportMember mbr)
            : base(mbr.User)
        {
            this.IsDeafened = mbr.IsDeafened;
            this.IsMuted = mbr.IsMuted;
            this.JoinedAt = mbr.JoinedAt;
            this.Nickname = mbr.Nickname;
            this._role_ids = mbr.Roles;
        }

        /// <summary>
        /// This users guild nickname
        /// </summary>
        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; internal set; }
        /// <summary>
        /// List of role ids
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<ulong> RoleIds => new ReadOnlyCollection<ulong>(this._role_ids);
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal List<ulong> _role_ids;
        /// <summary>
        /// Gets the list of roles associated with this member.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles => this._role_ids.Select(xid => this.Guild.Roles.FirstOrDefault(xr => xr.Id == xid));
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
        public DateTime JoinedAt { get; internal set; }
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

        internal ulong _guild_id = 0;

        public DiscordGuild Guild => this.Discord.Guilds[_guild_id];

        public Task<DiscordDmChannel> SendDmAsync() => this.Discord._rest_client.InternalCreateDM(this.Id);

        public Task SetMuteAsync(bool muted) => this.Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, muted: muted);

        public Task SetDeafAsync(bool deafened) => this.Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, deafened: deafened);

        public Task ModifyAsync(string nickname = null, List<ulong> roles = null, ulong voicechannel_id = 0) => this.Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, nickname, roles, voicechannel_id: voicechannel_id);

        public async Task GrantRoleAsync(ulong RoleID)
        {
            if (!this._role_ids.Contains(RoleID))
                this._role_ids.Add(RoleID);
            await Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, roles: this._role_ids);
        }

        public async Task TakeRoleAsync(ulong RoleID)
        {
            if(this._role_ids.Contains(RoleID))
                this._role_ids.Remove(RoleID);
            await Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, roles: this._role_ids);
        }
    }
}
