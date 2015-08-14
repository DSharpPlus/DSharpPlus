using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordUser
    {
        public string username { get; set; }
        public string id { get; set; }
    }

    public class DiscordMember
    {
        public DiscordUser user { get; set; }
        public DiscordMember() { user = new DiscordUser(); }
    }
}
