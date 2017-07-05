using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Objects.Transport;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;

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

        internal ulong _guild_id = 0;

        /// <summary>
        /// Gets the guild of which this member is a part of.
        /// </summary>
        public DiscordGuild Guild => this.Discord.Guilds[_guild_id];

        /// <summary>
        /// Creates a direct message channel to this member.
        /// </summary>
        /// <returns>Direct message channel to this member.</returns>
        public Task<DiscordDmChannel> CreateDmChannelAsync() => this.Discord._rest_client.InternalCreateDmAsync(this.Id);

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="content">Content of the message to send.</param>
        /// <param name="is_tts">Whether the message is to be read using TTS.</param>
        /// <param name="embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        public async Task<DiscordMessage> SendMessageAsync(string content, bool is_tts = false, DiscordEmbed embed = null)
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
        
        /// <summary>
        /// Returns a string representation of this member.
        /// </summary>
        /// <returns>String representation of this member.</returns>
        public override string ToString()
        {
            return string.Concat("Member ", this.Id, "; ", this.Username, "#", this.Discriminator, " (", this.DisplayName, ")");
        }
    }
}
