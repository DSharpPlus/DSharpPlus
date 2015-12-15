using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Events
{
    public class DiscordChannelDeleteEventArgs
    {
        public DiscordChannel ChannelDeleted { get; internal set; }
    }
}
