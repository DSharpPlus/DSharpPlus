using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp.Objects;
namespace DiscordSharp.Events
{
    public class DiscordVoiceUserSpeakingEventArgs : EventArgs
    {
        public DiscordMember UserSpeaking { get; internal set; }

        public DiscordChannel Channel { get; internal set; }

        //public DiscordServer Guild { get; internal set; }

        /// <summary>
        /// This is true if the user began speaking or false if they stopped speaking. 
        /// Eventually, these will be two seperate events.
        /// </summary>
        public bool Speaking { get; internal set; }

        public int Ssrc { get; internal set; }
    }
}
