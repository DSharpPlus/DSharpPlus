namespace DSharpPlus;

/// <summary>
/// Represents the duration in minutes to automatically archive a thread after recent activity.
/// </summary>
public enum AutoArchiveDuration : int
{
    /// <summary>
    /// Thread will auto-archive after one hour of inactivity.
    /// </summary>
    Hour = 60,

    /// <summary>
    /// Thread will auto-archive after one day of inactivity.
    /// </summary>
    Day = 1440,

    /// <summary>
    /// Thread will auto-archive after three days of inactivity.
    /// </summary>
    ThreeDays = 4320,

    /// <summary>
    /// Thread will auto-archive after one week of inactivity.
    /// </summary>
    Week = 10080
}
