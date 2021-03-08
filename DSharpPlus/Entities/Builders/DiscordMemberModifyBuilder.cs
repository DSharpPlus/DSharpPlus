using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents what the member should be changed to.
    /// </summary>
    public class DiscordMemberModifyBuilder
    {
        /// <summary>
        /// Gets the new nickname
        /// </summary>
        public Optional<string> Nickname { get; internal set; }

        /// <summary>
        /// Gets the list of roles.
        /// </summary>
        public IReadOnlyCollection<DiscordRole> Roles => this._Roles.Value;

        internal Optional<List<DiscordRole>> _Roles = new List<DiscordRole>();

        /// <summary>
        /// Gets whether this user should be muted
        /// </summary>
        public Optional<bool> Muted { get; internal set; }

        /// <summary>
        /// Gets whether this user should be deafened
        /// </summary>
        public Optional<bool> Deafened { get; internal set; }

        /// <summary>
        /// Gets the voice channel to move this user to, set to null to kick
        /// </summary>
        public Optional<DiscordChannel> VoiceChannel { get; internal set; }

        /// <summary>
        /// Get the reason given in audit logs
        /// </summary>
        public Optional<string> AuditLogReason { get; internal set; }

        /// <summary>
        /// Sets the new nickname of the member
        /// </summary>
        /// <param name="name">the nickname to give.</param>
        /// <returns></returns>
        public DiscordMemberModifyBuilder WithNickname(string name)
        {
            this.Nickname = name;

            return this;
        }

        /// <summary>
        /// Sets a role to add to the user.
        /// </summary>
        /// <param name="role">the role to give.</param>
        /// <returns></returns>
        public DiscordMemberModifyBuilder WithRole(DiscordRole role)
        {
            this._Roles.Value.Add(role);

            return this;
        }

        /// <summary>
        /// Sets roles to add to the user.
        /// </summary>
        /// <param name="roles">the roles to give.</param>
        /// <returns></returns>
        public DiscordMemberModifyBuilder WithRoles(IEnumerable<DiscordRole> roles)
        {
            this._Roles.Value.AddRange(roles);

            return this;
        }

        /// <summary>
        /// Sets if the user should be muted.
        /// </summary>
        /// <param name="mute">the mute value,</param>
        /// <returns></returns>
        public DiscordMemberModifyBuilder WithMute(bool mute)
        {
            this.Muted = mute;

            return this;
        }

        /// <summary>
        /// Sets if the user should be deafened.
        /// </summary>
        /// <param name="deafen">The deafen value.</param>
        /// <returns></returns>
        public DiscordMemberModifyBuilder WithDeafned(bool deafen)
        {
            this.Deafened = deafen;

            return this;
        }

        /// <summary>
        /// Sets the voice channel the member should be placed in.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public DiscordMemberModifyBuilder WithVoiceChannel(DiscordChannel channel)
        {
            this.VoiceChannel = channel;

            return this;
        }

        /// <summary>
        /// Sets the reason for the Change.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public DiscordMemberModifyBuilder WithAuditLogReason(string reason)
        {
            this.AuditLogReason = reason;

            return this;
        }

        /// <summary>
        /// Sends the builder to discord.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public async Task ModifyAysnc(DiscordMember member)
        {
            await member.ModifyAsync(this).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears the Whole builder back to default.
        /// </summary>
        public void Clear()
        {
            this.ClearNickname();
            this._Roles = Optional.FromNoValue<List<DiscordRole>>();
            this.Muted = Optional.FromNoValue<bool>();
            this.Deafened = Optional.FromNoValue<bool>();
            this.VoiceChannel = Optional.FromNoValue<DiscordChannel>();
            this.AuditLogReason = Optional.FromNoValue<string>();
        }

        internal void ClearNickname()
        {
            this.Nickname = Optional.FromNoValue<string>();
        }

        /// <summary>
        /// Validates all inputs sent to the builder.
        /// </summary>
        internal void Validate()
        {
            if (this.VoiceChannel.HasValue && this.VoiceChannel.Value != null && this.VoiceChannel.Value.Type != ChannelType.Voice)
                throw new ArgumentException("Given channel is not a voice channel.", nameof(this.VoiceChannel));
        }
    }
}
