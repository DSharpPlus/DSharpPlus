using System.IO;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models
{
    public class GuildEditModel : BaseEditModel
    {
        /// <summary>
        /// New guild name
        /// </summary>
        public Optional<string> Name { internal get; set; }
        /// <summary>
        /// New guild voice region
        /// </summary>
        public Optional<DiscordVoiceRegion> Region { internal get; set; }
        /// <summary>
        /// New guild icon
        /// </summary>
        public Optional<Stream> Icon { internal get; set; }
        /// <summary>
        /// New guild verification level
        /// </summary>
        public Optional<VerificationLevel> VerificationLevel { internal get; set; }
        /// <summary>
        /// New guild default message notification level
        /// </summary>
        public Optional<DefaultMessageNotifications> DefaultMessageNotifications { internal get; set; }
        /// <summary>
        /// New guild MFA level
        /// </summary>
        public Optional<MfaLevel> MfaLevel { internal get; set; }
        /// <summary>
        /// New guild explicit content filter level
        /// </summary>
        public Optional<ExplicitContentFilter> ExplicitContentFilter { internal get; set; }
        /// <summary>
        /// New AFK voice channel
        /// </summary>
        public Optional<DiscordChannel> AfkChannel { internal get; set; }
        /// <summary>
        /// New AFK timeout time in seconds
        /// </summary>
        public Optional<int> AfkTimeout { internal get; set; }
        /// <summary>
        /// New guild owner
        /// </summary>
        public Optional<DiscordMember> Owner { internal get; set; }
        /// <summary>
        /// New guild splash
        /// </summary>
        public Optional<Stream> Splash { internal get; set; }
        /// <summary>
        /// New guild system channel
        /// </summary>
        public Optional<DiscordChannel> SystemChannel { internal get; set; }
        
        internal GuildEditModel()
        {

        }
    }
}
