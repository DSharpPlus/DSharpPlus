using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Net.Models
{
    public class ChannelEditModel : BaseEditModel
    {
        public string Name { internal get; set; }
        public int? Position { internal get; set; }
        public string Topic { internal get; set; }
        public Optional<DiscordChannel> Parent { internal get; set; }
        public int? Bitrate { internal get; set; }
        public int? Userlimit { internal get; set; }
        
        internal ChannelEditModel()
        {

        }
    }
}
