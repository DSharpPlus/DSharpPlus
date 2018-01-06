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
        public string Nickname { internal get; set; }
        public List<DiscordRole> Roles { internal get; set; }
        public bool? Muted { internal get; set; }
        public bool? Deafened { internal get; set; }
        public DiscordChannel VoiceChannel { internal get; set; }
        
        internal MemberEditModel()
        {

        }
    }
}
