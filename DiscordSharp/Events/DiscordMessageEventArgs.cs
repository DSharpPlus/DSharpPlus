using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Events
{
    public class DiscordMessageEventArgs
    {
        public string username { get; internal set; }
        public string message { get; internal set; }
        public string ChannelID { get; internal set; }
        public string ServerID { get; internal set; }
        public string ChannelName { get; internal set; }
        public string ServerName { get; internal set; }

        public DiscordMessageEventArgs()
        {

        }
    }
}
