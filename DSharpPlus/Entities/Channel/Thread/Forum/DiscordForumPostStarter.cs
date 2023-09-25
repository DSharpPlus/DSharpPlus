namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents the return of creating a forum post.
    /// </summary>
    public sealed class DiscordForumPostStarter
    {
        /// <summary>
        /// The channel of the forum post.
        /// </summary>
        public DiscordThreadChannel Channel { get; internal set; }
        /// <summary>
        /// The message of the forum post.
        /// </summary>
        public DiscordMessage Message { get; internal set; }

        internal DiscordForumPostStarter() { }

        internal DiscordForumPostStarter(DiscordThreadChannel chn, DiscordMessage msg)
        {
            Channel = chn;
            Message = msg;
        }
    }
}
