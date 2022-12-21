namespace DSharpPlus.Entities;

/// <summary>
/// Duration in minutes to automatically archive the thread after recent activity, can be set to: 60, 1440, 4320, 10080.
/// </summary>
public enum DiscordThreadAutoArchiveDuration
{
    OneHour = 60,
    OneDay = 1440,
    ThreeDays = 4320,
    OneWeek = 10080
}
