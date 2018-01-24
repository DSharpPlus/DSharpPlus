using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Models
{
    public class MemberEditModel : BaseEditModel
    {
        public Optional<string> Nickname { internal get; set; }
        public Optional<List<DiscordRole>> Roles { internal get; set; }
        public Optional<bool> Muted { internal get; set; }
        public Optional<bool> Deafened { internal get; set; }
        public Optional<DiscordChannel> VoiceChannel { internal get; set; }
        
        internal MemberEditModel()
        {

        }
    }
}
