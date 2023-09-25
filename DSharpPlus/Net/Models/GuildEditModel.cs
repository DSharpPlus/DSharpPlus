using System.Collections.Generic;
using System.IO;
using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models
{
    public class GuildEditModel : BaseEditModel
    {
        /// <summary>
        /// The new guild name.
        /// </summary>
        public Optional<string> Name { internal get; set; }

        /// <summary>
        /// The new guild voice region.
        /// </summary>
        public Optional<DiscordVoiceRegion> Region { internal get; set; }

        /// <summary>
        /// The new guild icon.
        /// </summary>
        public Optional<Stream> Icon { internal get; set; }

        /// <summary>
        /// The new guild verification level.
        /// </summary>
        public Optional<VerificationLevel> VerificationLevel { internal get; set; }

        /// <summary>
        /// The new guild default message notification level.
        /// </summary>
        public Optional<DefaultMessageNotifications> DefaultMessageNotifications { internal get; set; }

        /// <summary>
        /// The new guild MFA level.
        /// </summary>
        public Optional<MfaLevel> MfaLevel { internal get; set; }

        /// <summary>
        /// The new guild explicit content filter level.
        /// </summary>
        public Optional<ExplicitContentFilter> ExplicitContentFilter { internal get; set; }

        /// <summary>
        /// The new AFK voice channel.
        /// </summary>
        public Optional<DiscordChannel> AfkChannel { internal get; set; }

        /// <summary>
        /// The new AFK timeout time in seconds.
        /// </summary>
        public Optional<int> AfkTimeout { internal get; set; }

        /// <summary>
        /// The new guild owner.
        /// </summary>
        public Optional<DiscordMember> Owner { internal get; set; }

        /// <summary>
        /// The new guild splash.
        /// </summary>
        public Optional<Stream> Splash { internal get; set; }

        /// <summary>
        /// The new guild system channel.
        /// </summary>
        public Optional<DiscordChannel> SystemChannel { internal get; set; }

        /// <summary>
        /// The new guild rules channel.
        /// </summary>
        public Optional<DiscordChannel> RulesChannel { internal get; set; }

        /// <summary>
        /// The new guild public updates channel.
        /// </summary>
        public Optional<DiscordChannel> PublicUpdatesChannel { internal get; set; }

        /// <summary>
        /// The new guild preferred locale.
        /// </summary>
        public Optional<string> PreferredLocale { internal get; set; }

        /// <summary>
        /// The new description of the guild
        /// </summary>
        public Optional<string> Description { get; set; }

        /// <summary>
        /// The new discovery splash image of the guild
        /// </summary>
        public Optional<string> DiscoverySplash { get; set; }

        /// <summary>
        /// A list of <see href="https://discord.com/developers/docs/resources/guild#guild-object-guild-features">guild features</see>
        /// </summary>
        public Optional<IEnumerable<string>> Features { get; set; }

        /// <summary>
        /// The new banner of the guild
        /// </summary>
        public Optional<Stream> Banner { get; set; }

        /// <summary>
        /// The new system channel flags for the guild
        /// </summary>
        public Optional<SystemChannelFlags> SystemChannelFlags { get; set; }

        internal GuildEditModel()
        {

        }
    }
}
