using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Events
{
    public class DiscordMessageEventArgs
    {
        public string message { get; internal set; }
        public DiscordMember author { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordMessageType MessageType { get; internal set; }

        public DiscordMessageEventArgs()
        {}
    }
}
