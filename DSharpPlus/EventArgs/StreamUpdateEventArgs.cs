using DSharpPlus.Entities;

namespace DSharpPlus.EventArgs
{
    /// <summary>
    /// Represents arguments for <see cref="DiscordClient.VoiceStateUpdated"/> event.
    /// </summary>
    public class StreamUpdateEventArgs : DiscordEventArgs
    {
        /// <summary>
        /// Gets the token associated with this new or updated stream.
        /// </summary>
        public string Token {get; set;}

        /// <summary>
        /// Gets the key associated with this new or updated stream.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the endpoint related to this stream.
        /// </summary>
        public string Endpoint { get; set; }


        public string RtcServerId { get; set; }
 
        public bool Deafened { get; set; }

        internal StreamUpdateEventArgs() : base() { }
    }

    public class StreamCreateEventArgs: DiscordEventArgs
    {
        public string Region { get; set; }

        public string RtcServerId { get; set; }

        public ulong[] Viewers { get; set; }

        public string Key { get; set; }

        public bool Paused { get; set; }
    }
}
