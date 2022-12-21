namespace DSharpPlus.Core.Enums
{
    public enum DiscordActivityType
    {
        /// <summary>
        /// Format: Playing {name}
        /// Example: "Playing Rocket League"
        /// </summary>
        Game = 0,

        /// <summary>
        /// Format: Streaming {details}
        /// Example: "Streaming Rocket League"
        /// </summary>
        /// <remarks>
        /// The streaming type currently only supports Twitch and YouTube. Only <c>https://twitch.tv/</c> and <c>https://youtube.com/</c> urls will work.
        /// </remarks>
        Streaming = 1,

        /// <summary>
        /// Format: Listening to {name}
        /// Example: "Listening to Spotify"
        /// </summary>
        Listening = 2,

        /// <summary>
        /// Format: Watching {name}
        /// Example: "Watching YouTube Together"
        /// </summary>
        Watching = 3,

        /// <summary>
        /// Format: {emoji} {name}
        /// Example: ":smiley: I am cool"
        /// </summary>
        Custom = 4,

        /// <summary>
        /// Format: Competing in {name}
        /// Example: "Competing in Arena World Champions"
        /// </summary>
        Competing = 5
    }
}
