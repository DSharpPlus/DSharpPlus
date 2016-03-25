using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Objects
{
    public class DiscordUserInformation
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
        public DiscordUserInformation()
        {}

        public DiscordUserInformation Copy()
        {
            return new DiscordUserInformation
            {
                Email = this.Email,
                Password = this.Password,
                Username = this.Username,
                Avatar = this.Avatar
            };
        }
    }

}
