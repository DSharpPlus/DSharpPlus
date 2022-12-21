namespace DSharpPlus.Entities.Internal;

public sealed record InternalGuildWidgetSettings
{
    /// <summary>
    /// Whether the widget is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// The widget channel id.
    /// </summary>
    public InternalSnowflake? ChannelId { get; init; }
}
