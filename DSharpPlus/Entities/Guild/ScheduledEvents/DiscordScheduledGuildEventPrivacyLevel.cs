namespace DSharpPlus.Entities;

/// <summary>
/// Privacy level for a <see cref="DiscordScheduledGuildEvent"/>.
/// </summary>
public enum DiscordScheduledGuildEventPrivacyLevel
{
    /// <summary>
    /// This event is public.
    /// </summary>
    Public = 1,
    /// <summary>
    /// This event is only available to the members of the guild.
    /// </summary>
    GuildOnly = 2,
}
