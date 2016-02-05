using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Objects
{
    public class DiscordUserInformation
    {
        public string email { get; set; }
        public string password { get; set; }
        public string username { get; set; }
        public string avatar { get; set; }
        public DiscordUserInformation()
        {}

        public DiscordUserInformation Copy()
        {
            return new DiscordUserInformation
            {
                email = this.email,
                password = this.password,
                username = this.username,
                avatar = this.avatar
            };
        }
    }

}
