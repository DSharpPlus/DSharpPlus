using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordUserInformation
    {
        public string email { get; set; }
        public string password { get; set; }
        public string username { get; set; }
        public string avatar { get; set; }
        public DiscordUserInformation()
        {}
    }
}
