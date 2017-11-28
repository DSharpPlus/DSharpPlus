using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Models
{
    public class GuildEditModel
    {
        public string Name { internal get; set; }
        public DiscordVoiceRegion Region { internal get; set; }
        public Stream Icon { internal get; set; }
        public VerificationLevel VerificationLevel { internal get; set; }
        public DefaultMessageNotifications DefaultMessageNotifications { internal get; set; }
        public MfaLevel MfaLevel { internal get; set; }
        public ExplicitContentFilter ExplicitContentFilter { internal get; set; }
        public DiscordChannel AfkChannel { internal get; set; }
        public int? AfkTimeout { internal get; set; }
        public DiscordMember Owner { internal get; set; }
        public Stream Splash { internal get; set; }
        public string AuditLogReason { internal get; set; }

        internal GuildEditModel()
        {

        }
    }
}
