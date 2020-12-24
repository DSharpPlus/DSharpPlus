using DSharpPlus.EventArgs;

namespace DSharpPlus.VideoNext.Entities
{
    internal class VideoStateData: DiscordEventArgs
    {
        /// <summary>
        /// The token associated with this stream.
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Gets the key associated with this new/updated stream.
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// Gets the endpoint for this stream.
        /// </summary>
        public string Endpoint { get; set; }
        
        /// <summary>
        /// The RTC Server ID for this stream.
        /// </summary>
        public string RtcServerId { get; set; }
        
        public bool Deafened { get; set; }

        internal VideoStateData() : base() { }
    }
}