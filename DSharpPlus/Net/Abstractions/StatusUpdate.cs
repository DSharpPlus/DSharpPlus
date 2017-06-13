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
        [JsonProperty("idle_since", NullValueHandling = NullValueHandling.Include)]
        public long? IdleSince { get; set; }

        /// <summary>
        /// Gets or sets whether the user is AFK.
        /// </summary>
        [JsonProperty("afk")]
        public bool IsAFK { get; set; }

        /// <summary>
        /// Gets or sets the status of the user.
        /// </summary>
        [JsonProperty("status")]
        public UserStatus Status { get; set; } = UserStatus.Online;
        
        /// <summary>
        /// Gets or sets the game the user is playing.
        /// </summary>
        [JsonProperty("game", NullValueHandling = NullValueHandling.Ignore)]
        public Game Game { get; set; }
    }
}

namespace DSharpPlus
{
    /// <summary>
    /// Represents user status.
    /// </summary>
    public enum UserStatus : int
    {
        /// <summary>
        /// User is offline.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// User is online.
        /// </summary>
        Online = 1,

        /// <summary>
        /// User is idle.
        /// </summary>
        Idle = 2,

        /// <summary>
        /// User is away from keyboard.
        /// </summary>
        AFK = 3,

        /// <summary>
        /// User asked not to be disturbed.
        /// </summary>
        DoNotDisturb = 4,

        /// <summary>
        /// User is invisible. They will appear as Offline to anyone but themselves.
        /// </summary>
        Invisible = 5
    }

    /// <summary>
    /// Represents a game that a user is playing.
    /// </summary>
    public sealed class Game
    {
        /// <summary>
        /// Creates a new, empty instance of a <see cref="Game"/>.
        /// </summary>
        public Game() { }

        /// <summary>
        /// Creates a new instance of a <see cref="Game"/> with specified name.
        /// </summary>
        /// <param name="name"></param>
        public Game(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the game the user is playing.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stream URI, if applicable.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the livesteam type.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public GameStreamType? StreamType { get; set; }
    }

    /// <summary>
    /// Determines the livestream type for a game.
    /// </summary>
    public enum GameStreamType
    {
        /// <summary>
        /// The game is not being streamed.
        /// </summary>
        NoStream = 0,

        /// <summary>
        /// The game is being streamed on Twitch.
        /// </summary>
        Twitch = 1
    }
}
