using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordSubChannel
    {
        public string type { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    public class DiscordServer
    {
        public string id { get; set; }
        public string name { get; set; }
        public string owner_id { get; set; }
        public List<DiscordSubChannel> channels { get; set; }
        public List<DiscordMember> members { get; set; }

        public DiscordServer()
        {
            channels = new List<DiscordSubChannel>();
            members = new List<DiscordMember>();
        }
    }
}
