using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Objects.Transport;
using Newtonsoft.Json;
using System.Linq;

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
            this.Roles = mbr.Roles;
        }

        /// <summary>
        /// This users guild nickname
        /// </summary>
        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; internal set; }
        /// <summary>
        /// List of role object id's
        /// </summary>
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<ulong> Roles { get; internal set; }
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

        public ulong GuildId = 0;

        public Task<DiscordDmChannel> SendDmAsync() => this.Discord._rest_client.InternalCreateDM(this.Id);

        public Task SetMuteAsync(bool muted) => this.Discord._rest_client.InternalModifyGuildMember(GuildId, Id, muted: muted);

        public Task SetDeafAsync(bool deafened) => this.Discord._rest_client.InternalModifyGuildMember(GuildId, Id, deafened: deafened);

        public Task ModifyAsync(string nickname = null, List<ulong> roles = null, ulong voicechannel_id = 0) => this.Discord._rest_client.InternalModifyGuildMember(GuildId, Id, nickname, roles, voicechannel_id: voicechannel_id);

        public async Task GrantRoleAsync(ulong RoleID)
        {
            var roles = Roles;
            roles.Add(RoleID);
            await Discord._rest_client.InternalModifyGuildMember(GuildId, Id, roles: roles);
        }

        public async Task TakeRoleAsync(ulong RoleID)
        {
            var roles = Roles;
            roles.Add(RoleID);
            await Discord._rest_client.InternalModifyGuildMember(GuildId, Id, roles: roles);
        }

        public int GetNameColor()
        {
            List<DiscordRole> r = this.Discord.Guilds[GuildId].Roles.FindAll(x => Roles.Contains(x.Id));
            var rr = r.OrderBy(x => x.Position);
            return rr.ToList().Last().Color;
        }

    }
}
