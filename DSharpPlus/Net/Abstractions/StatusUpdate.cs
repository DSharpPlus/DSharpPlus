using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Represents data for websocket status update payload.
    /// </summary>
    internal sealed class StatusUpdate
    {
        /// <summary>
        /// Gets or sets the unix millisecond timestamp of when the user went idle.
        /// </summary>
        [JsonProperty("since", NullValueHandling = NullValueHandling.Include)]
        public long? IdleSince { get; set; }

        /// <summary>
        /// Gets or sets whether the user is AFK.
        /// </summary>
        [JsonProperty("afk")]
        public bool IsAFK { get; set; }

        /// <summary>
        /// Gets or sets the status of the user.
        /// </summary>
        [JsonIgnore]
        public UserStatus Status { get; set; } = UserStatus.Online;

        [JsonProperty("status")]
        internal string StatusString
        {
            get
            {
                switch (this.Status)
                {
                    case UserStatus.Online:
                        return "online";
                        
                    case UserStatus.Idle:
                        return "idle";

                    case UserStatus.DoNotDisturb:
                        return "dnd";

                    case UserStatus.Invisible:
                    case UserStatus.Offline:
                        return "invisible";
                }

                return "online";
            }
        }

        /// <summary>
        /// Gets or sets the game the user is playing.
        /// </summary>
        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public TransportActivity Activity { get; set; }

        internal DiscordActivity _activity;
    }
}