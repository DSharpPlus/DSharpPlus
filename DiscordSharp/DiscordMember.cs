using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp
{
    public class DiscordUser
    {
        public string username { get; internal set; }
        public string id { get; internal set; }
        public string discriminator { get; internal set; }
        public string avatar { get; internal set; }
        public bool verified { get; internal set; }
        public string email { get; internal set; }

        /*
        Voice only
        */
        /// <summary>
        /// Whether or not the member can speak/mic enabled in the voice channel.
        /// </summary>
        public bool mute { get; internal set; } = false;
        /// <summary>
        /// Whether or not the member can hear others in the voice channel.
        /// </summary>
        public bool deaf { get; internal set; } = false;
    }

    public class DiscordMember
    {
        public DiscordUser user { get; set; }
        public DiscordMember() { user = new DiscordUser(); }
        public List<DiscordRole> roles { get; set; }

        public DiscordServer parent { get; internal set; }
    }
}
