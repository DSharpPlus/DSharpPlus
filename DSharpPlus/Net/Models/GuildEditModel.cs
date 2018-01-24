using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Models
{
    public class GuildEditModel : BaseEditModel
    {
        public Optional<string> Name { internal get; set; }
        public Optional<DiscordVoiceRegion> Region { internal get; set; }
        public Optional<Stream> Icon { internal get; set; }
        public Optional<VerificationLevel> VerificationLevel { internal get; set; }
        public Optional<DefaultMessageNotifications> DefaultMessageNotifications { internal get; set; }
        public Optional<MfaLevel> MfaLevel { internal get; set; }
        public Optional<ExplicitContentFilter> ExplicitContentFilter { internal get; set; }
        public Optional<DiscordChannel> AfkChannel { internal get; set; }
        public Optional<int> AfkTimeout { internal get; set; }
        public Optional<DiscordMember> Owner { internal get; set; }
        public Optional<Stream> Splash { internal get; set; }
        public Optional<DiscordChannel> SystemChannel { internal get; set; }
        
        internal GuildEditModel()
        {

        }
    }
}
