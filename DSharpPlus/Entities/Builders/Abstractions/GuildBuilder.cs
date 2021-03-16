using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.Entities.Abstractions
{
    /// <summary>
    /// Represents the Guild that will be Created or Modified.
    /// </summary>
    public abstract class GuildBuilder<T>
    {
        /// <summary>
        /// <para>Gets or Sets the Name of the guild to be sent.</para>
        /// <para>This must be between 2 and 100 Characters and is Required for Guild Creation.</para>
        /// </summary>
        public string Name
        {
            get => this._name;
            set
            {
                if (value != null && value.Length <= 2 && value.Length >= 100)
                    throw new ArgumentException("Name must be between 2 and 100 Characters.", nameof(value));
                this._name = value;
            }
        }
        private string _name;

        /// <summary>
        /// Gets the Voice Region the Guild should be set in.
        /// </summary>
        public Optional<string> VoiceRegionId { get; internal set; }

        /// <summary>
        /// <para>Gets the Icon of the guild in a base64 format.</para>
        /// </summary>
        public Optional<string> Icon { get; internal set; }

        /// <summary>
        /// Gets the Verification Level of the guild.
        /// </summary>
        public Optional<VerificationLevel> VerificationLevel { get; internal set; }

        /// <summary>
        /// Gets the Default Message Notification Level of the guild.
        /// </summary>
        public Optional<DefaultMessageNotifications> DefaultMessageNotificationLevel { get; internal set; }

        /// <summary>
        /// Gets the Explicit Content Filter Level of the guild.
        /// </summary>
        public Optional<ExplicitContentFilter> ExplicitContentFilterLevel { get; internal set; }

        /// <summary>
        /// Gets the Afk Channel Id of the guild.
        /// </summary>
        public Optional<ulong> AfkChannelId { get; internal set; }

        /// <summary>
        /// Gets the Afk Timeout of the guild.
        /// </summary>
        public Optional<int> AfkTimeout { get; internal set; }

        /// <summary>
        /// Gets the System Channel
        /// </summary>
        public Optional<ulong> SystemChannelId { get; internal set; }

        /// <summary>
        /// Gets the AuditLog Reason for modifing the guild.
        /// </summary>
        public Optional<string> AuditLogReason { get; internal set; }

        /// <summary>
        /// <para>Sets the Name of the guild to be sent.</para>
        /// <para>This must be between 2 and 100 Characters and is Required for Guild Creation.</para>
        /// </summary>
        /// <param name="name">The name of the guild.</param>
        /// <returns></returns>
        public T WithName(string name)
        {
            this.Name = name;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Voice Region the Guild should be set in.
        /// </summary>
        /// <param name="region">The region the guild should be set in.</param>
        /// <returns></returns>
        public T WithVoiceRegion(DiscordVoiceRegion region)
        {
            this.VoiceRegionId = region.Id;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Icon of the guild.  Must be in a Base64 string that is 128x128.
        /// </summary>
        /// <param name="icon">The icon for the guild.</param>
        /// <returns></returns>
        public T WithIcon(string icon)
        {
            this.Icon = icon;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Verification level of the guild.
        /// </summary>
        /// <param name="verificationLevel">The verification level.</param>
        /// <returns></returns>
        public T WithVerificationLevel(VerificationLevel verificationLevel)
        {
            this.VerificationLevel = verificationLevel;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Default Message Notification Level
        /// </summary>
        /// <param name="defaultMessageNotifications">The Default Notification Level</param>
        /// <returns></returns>
        public T WithDefaultMessageNotificationLevel(DefaultMessageNotifications defaultMessageNotifications)
        {
            this.DefaultMessageNotificationLevel = defaultMessageNotifications;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Explicit Content Filter Level of the guild.
        /// </summary>
        /// <param name="explicitContentFilter">The Explicit Content Filter</param>
        /// <returns></returns>
        public T WithExplicitContentFilterLevel(ExplicitContentFilter explicitContentFilter)
        {
            this.ExplicitContentFilterLevel = explicitContentFilter;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Afk channel of the guild. 
        /// </summary>
        /// <param name="channelId">the Id of the channel.</param>
        /// <returns></returns>
        public T WithAfkChannelId(ulong channelId)
        {
            this.AfkChannelId = channelId;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the Afk timeout of the guild.
        /// </summary>
        /// <param name="timeout">The timeout value.</param>
        /// <returns></returns>
        public T WithAfkTimeout(int timeout)
        {
            this.AfkTimeout = timeout;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the System Channel Id of the guild.
        /// </summary>
        /// <param name="channelId">The Id of the channel.</param>
        /// <returns></returns>
        public T WithSystemChannelId(ulong channelId)
        {
            this.SystemChannelId = channelId;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Sets the reason for the Change.
        /// </summary>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public T WithAuditLogReason(string reason)
        {
            this.AuditLogReason = reason;

            return (T)Convert.ChangeType(this, typeof(T));
        }

        /// <summary>
        /// Performs validation logic to verify all the input is valid before sending it off to discord.
        /// </summary>
        internal abstract void Validate();

        /// <summary>
        /// Clears the Builder to be used again.
        /// </summary>
        public virtual void Clear()
        {
            this._name = "";
            this.VoiceRegionId = Optional.FromNoValue<string>();
            this.Icon = Optional.FromNoValue<string>();
            this.VerificationLevel = Optional.FromNoValue<VerificationLevel>();
            this.ExplicitContentFilterLevel = Optional.FromNoValue<ExplicitContentFilter>();
            this.AfkChannelId = Optional.FromNoValue<ulong>();
            this.AfkTimeout = Optional.FromNoValue<int>();
            this.SystemChannelId = Optional.FromNoValue<ulong>();
        }
    }
}
