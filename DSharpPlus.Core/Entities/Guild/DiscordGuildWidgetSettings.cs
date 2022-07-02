namespace DSharpPlus.Core.Entities
{
    public sealed record DiscordGuildWidgetSettings
    {
        /// <summary>
        /// Whether the widget is enabled.
        /// </summary>
        public bool Enabled { get; init; }

        /// <summary>
        /// The widget channel id.
        /// </summary>
        public DiscordSnowflake? ChannelId { get; init; }
    }
}
