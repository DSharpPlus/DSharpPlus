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

        internal ulong _guild_id = 0;

        public DiscordGuild Guild => this.Discord.Guilds[_guild_id];

        public Task<DiscordDmChannel> SendDmAsync() => this.Discord._rest_client.InternalCreateDM(this.Id);

        public Task SetMuteAsync(bool muted) => this.Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, muted: muted);

        public Task SetDeafAsync(bool deafened) => this.Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, deafened: deafened);

        public Task ModifyAsync(string nickname = null, List<ulong> roles = null, ulong voicechannel_id = 0) => this.Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, nickname, roles, voicechannel_id: voicechannel_id);

        public async Task GrantRoleAsync(ulong RoleID)
        {
            if (!Roles.Contains(RoleID))
                Roles.Add(RoleID);
            await Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, roles: Roles);
        }

        public async Task TakeRoleAsync(ulong RoleID)
        {
            if(Roles.Contains(RoleID))
                Roles.Remove(RoleID);
            await Discord._rest_client.InternalModifyGuildMember(_guild_id, Id, roles: Roles);
        }

        public int GetNameColor()
        {
            var r = this.Discord.Guilds[_guild_id].Roles.FindAll(x => Roles.Contains(x.Id) && x.Color != 0).OrderBy(x => x.Position);
            return r.Last().Color;
        }

    }
}
